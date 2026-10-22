using SimpleDocumentForm.Models;

namespace SimpleDocumentForm;

public static class FormCalculator
{
    /// <summary>
    /// Calculates the value of the specified field on a document form.
    /// </summary>
    /// <param name="form">The validated document form.</param>
    /// <param name="userValues">Values entered by the user, keyed by user field name.</param>
    /// <param name="fieldName">The name of the field whose value should be calculated.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="ArgumentException">
    /// The field does not exist on the form, or a required user field value is missing.
    /// </exception>
    public static decimal CalculateFieldValue(
        DocumentForm form,
        IReadOnlyDictionary<string, decimal> userValues,
        string fieldName)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(userValues);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        if (!form.TryGetField(fieldName, out _))
        {
            throw new ArgumentException(
                $"Field '{fieldName}' does not exist on form '{form.Name}'.",
                nameof(fieldName));
        }

        var cache = new Dictionary<string, decimal>(StringComparer.Ordinal);
        return Evaluate(form, userValues, fieldName, cache);
    }

    private static decimal Evaluate(
        DocumentForm form,
        IReadOnlyDictionary<string, decimal> userValues,
        string fieldName,
        Dictionary<string, decimal> cache)
    {
        if (cache.TryGetValue(fieldName, out var cachedValue))
        {
            return cachedValue;
        }

        form.TryGetField(fieldName, out var field);

        var value = field switch
        {
            UserField userField => userValues.TryGetValue(userField.Name, out var userValue)
                ? userValue
                : throw new ArgumentException(
                    $"No value was provided for user field '{userField.Name}'.",
                    nameof(userValues)),
            CalculatedField calculatedField =>
                Evaluate(form, userValues, calculatedField.LeftOperand, cache) +
                Evaluate(form, userValues, calculatedField.RightOperand, cache),
            _ => throw new NotSupportedException($"Unsupported field type '{field?.GetType().Name}'."),
        };

        cache[fieldName] = value;
        return value;
    }
}
