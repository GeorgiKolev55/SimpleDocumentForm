using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Api.Forms;

/// <summary>
/// Holds the predefined forms in memory. Definitions that fail validation are kept as invalid forms
/// (with the reason) and reported with a warning. Definitions with a duplicate id are skipped.
/// </summary>
public sealed class InMemoryFormRepository : IFormRepository
{
    private readonly Dictionary<int, RegisteredForm> _formsById = new();
    private readonly IReadOnlyList<RegisteredForm> _forms;

    public InMemoryFormRepository(IEnumerable<FormDefinition> definitions, ILogger<InMemoryFormRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(logger);

        foreach (var definition in definitions)
        {
            if (_formsById.ContainsKey(definition.Id))
            {
                logger.LogWarning(
                    "Skipping form '{FormName}': id {FormId} is already in use.",
                    definition.Name,
                    definition.Id);
                continue;
            }

            _formsById.Add(definition.Id, Register(definition, logger));
        }

        _forms = _formsById.Values.OrderBy(form => form.Id).ToList().AsReadOnly();
    }

    public IReadOnlyList<RegisteredForm> GetAll() => _forms;

    public RegisteredForm? GetById(int id) => _formsById.GetValueOrDefault(id);

    private static RegisteredForm Register(FormDefinition definition, ILogger logger)
    {
        FormDefinitionError error;
        try
        {
            return RegisteredForm.Valid(definition.Id, new DocumentForm(definition.Name, definition.Fields));
        }
        catch (CircularDependencyException ex)
        {
            error = new FormDefinitionError(ex.Message, ex.Cycle);
        }
        catch (ArgumentException ex)
        {
            error = new FormDefinitionError(ex.Message);
        }

        logger.LogWarning(
            "Form '{FormName}' (id {FormId}) is invalid and cannot be calculated: {Reason}",
            definition.Name,
            definition.Id,
            error.Message);

        return RegisteredForm.Invalid(definition, error);
    }
}
