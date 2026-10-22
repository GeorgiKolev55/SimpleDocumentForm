using System.Diagnostics.CodeAnalysis;

namespace SimpleDocumentForm.Models;

/// <summary>
/// A predefined document form consisting of user-entered and calculated fields.
/// The form is validated on creation: field names must be unique, calculated fields
/// must reference existing fields, and there must be no circular dependencies.
/// </summary>
public sealed class DocumentForm
{
    private readonly Dictionary<string, FormField> _fieldsByName = new(StringComparer.Ordinal);

    public DocumentForm(string name, IEnumerable<FormField> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(fields);

        var fieldList = new List<FormField>();
        foreach (var field in fields)
        {
            if (field is null)
            {
                throw new ArgumentException("Fields must not contain null entries.", nameof(fields));
            }

            if (!_fieldsByName.TryAdd(field.Name, field))
            {
                throw new ArgumentException($"Duplicate field name '{field.Name}'.", nameof(fields));
            }

            fieldList.Add(field);
        }

        Name = name;
        Fields = fieldList.AsReadOnly();

        EnsureOperandsExist();
        EnsureNoCircularDependencies();
    }

    public string Name { get; }

    public IReadOnlyList<FormField> Fields { get; }

    public bool TryGetField(string name, [NotNullWhen(true)] out FormField? field) =>
        _fieldsByName.TryGetValue(name, out field);

    private void EnsureOperandsExist()
    {
        foreach (var field in Fields.OfType<CalculatedField>())
        {
            foreach (var operand in new[] { field.LeftOperand, field.RightOperand })
            {
                if (!_fieldsByName.ContainsKey(operand))
                {
                    throw new ArgumentException(
                        $"Calculated field '{field.Name}' references unknown field '{operand}'.",
                        "fields");
                }
            }
        }
    }

    private void EnsureNoCircularDependencies()
    {
        var states = new Dictionary<string, VisitState>(StringComparer.Ordinal);
        var path = new List<string>();

        foreach (var field in Fields)
        {
            Visit(field.Name, states, path);
        }
    }

    private void Visit(string fieldName, Dictionary<string, VisitState> states, List<string> path)
    {
        if (states.TryGetValue(fieldName, out var state))
        {
            if (state == VisitState.Visited)
            {
                return;
            }

            var cycleStart = path.IndexOf(fieldName);
            var cycle = path.Skip(cycleStart).Append(fieldName).ToList();
            throw new CircularDependencyException(cycle);
        }

        if (_fieldsByName[fieldName] is not CalculatedField calculated)
        {
            states[fieldName] = VisitState.Visited;
            return;
        }

        states[fieldName] = VisitState.Visiting;
        path.Add(fieldName);

        Visit(calculated.LeftOperand, states, path);
        Visit(calculated.RightOperand, states, path);

        path.RemoveAt(path.Count - 1);
        states[fieldName] = VisitState.Visited;
    }

    private enum VisitState
    {
        Visiting,
        Visited,
    }
}
