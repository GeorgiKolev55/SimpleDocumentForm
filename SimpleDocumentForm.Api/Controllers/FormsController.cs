using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using SimpleDocumentForm.Api.Contracts;
using SimpleDocumentForm.Api.Forms;
using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Api.Controllers;

[ApiController]
[Route("api/forms")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class FormsController(IFormRepository formRepository) : ControllerBase
{
    /// <summary>
    /// Lists all predefined forms, including invalid ones (with the reason they cannot be calculated).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<FormSummaryResponse>> GetForms() =>
        Ok(formRepository.GetAll().Select(form => form.ToSummaryResponse()).ToList());

    /// <summary>
    /// Gets the form information needed to fill in its user fields. Invalid forms are returned with
    /// their status and error so clients can explain why they cannot be calculated.
    /// </summary>
    [HttpGet("{formId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<FormResponse> GetForm(int formId)
    {
        var registeredForm = formRepository.GetById(formId);
        if (registeredForm is null)
        {
            return FormNotFound(formId);
        }

        return Ok(registeredForm.ToResponse());
    }

    /// <summary>
    /// Completes a form: calculates the values of all fields based on the user's input.
    /// </summary>
    [HttpPost("{formId:int}/completions")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<CompletedFormResponse> CompleteForm(int formId, CompleteFormRequest request)
    {
        var registeredForm = formRepository.GetById(formId);
        if (registeredForm is null)
        {
            return FormNotFound(formId);
        }

        if (!registeredForm.IsValid)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Form cannot be calculated.",
                detail: $"Form '{registeredForm.Name}' cannot be calculated: {registeredForm.Error?.Message}");
        }

        var form = registeredForm.Form;
        ValidateUserValues(form, request.Values);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var fields = new List<CompletedFieldResponse>(form.Fields.Count);
        foreach (var field in form.Fields)
        {
            decimal value;
            try
            {
                value = FormCalculator.CalculateFieldValue(form, request.Values, field.Name);
            }
            catch (OverflowException)
            {
                ModelState.AddModelError(
                    nameof(request.Values),
                    $"The value of field '{field.Name}' exceeds the supported numeric range.");
                return ValidationProblem(ModelState);
            }

            fields.Add(new CompletedFieldResponse(field.Name, field.ToFieldType(), value));
        }

        return Ok(new CompletedFormResponse(registeredForm.Id, form.Name, fields));
    }

    private void ValidateUserValues(DocumentForm form, IReadOnlyDictionary<string, decimal> values)
    {
        foreach (var fieldName in values.Keys)
        {
            if (!form.TryGetField(fieldName, out var field))
            {
                ModelState.AddModelError(
                    $"{nameof(CompleteFormRequest.Values)}.{fieldName}",
                    $"Field '{fieldName}' does not exist on form '{form.Name}'.");
            }
            else if (field is CalculatedField)
            {
                ModelState.AddModelError(
                    $"{nameof(CompleteFormRequest.Values)}.{fieldName}",
                    $"Field '{fieldName}' is calculated and cannot be set.");
            }
        }

        foreach (var userField in form.Fields.OfType<UserField>())
        {
            if (!values.ContainsKey(userField.Name))
            {
                ModelState.AddModelError(
                    $"{nameof(CompleteFormRequest.Values)}.{userField.Name}",
                    $"A value is required for user field '{userField.Name}'.");
            }
        }
    }

    private ObjectResult FormNotFound(int formId) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Form not found.",
            detail: $"Form with id '{formId}' does not exist.");
}
