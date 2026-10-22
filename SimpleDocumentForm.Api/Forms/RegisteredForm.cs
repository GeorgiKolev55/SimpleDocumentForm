using System.Diagnostics.CodeAnalysis;
using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Api.Forms;

/// <summary>
/// A predefined form available through the API. Valid forms can be calculated. Invalid forms are
/// still exposed, together with the reason, so clients can explain why they cannot be used.
/// </summary>
public sealed class RegisteredForm
{
    private RegisteredForm(
        int id,
        string name,
        IReadOnlyList<FormField> fields,
        DocumentForm? form,
        FormDefinitionError? error)
    {
        Id = id;
        Name = name;
        Fields = fields;
        Form = form;
        Error = error;
    }

    public int Id { get; }

    public string Name { get; }

    public IReadOnlyList<FormField> Fields { get; }

    /// <summary>
    /// The validated form, or <see langword="null"/> when the definition is invalid.
    /// </summary>
    public DocumentForm? Form { get; }

    /// <summary>
    /// Why the definition is invalid, or <see langword="null"/> when the form is valid.
    /// </summary>
    public FormDefinitionError? Error { get; }

    [MemberNotNullWhen(true, nameof(Form))]
    public bool IsValid => Form is not null;

    public static RegisteredForm Valid(int id, DocumentForm form)
    {
        ArgumentNullException.ThrowIfNull(form);
        return new RegisteredForm(id, form.Name, form.Fields, form, error: null);
    }

    public static RegisteredForm Invalid(FormDefinition definition, FormDefinitionError error)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(error);

        // Null entries are one of the reasons a definition can be invalid; they cannot be displayed.
        var fields = definition.Fields.OfType<FormField>().ToList().AsReadOnly();
        return new RegisteredForm(definition.Id, definition.Name, fields, form: null, error);
    }
}

/// <summary>
/// Describes why a form definition is invalid.
/// </summary>
/// <param name="Message">A human-readable reason.</param>
/// <param name="Cycle">For circular dependencies, the fields forming the cycle (e.g. A, C, A).</param>
public sealed record FormDefinitionError(string Message, IReadOnlyList<string>? Cycle = null);
