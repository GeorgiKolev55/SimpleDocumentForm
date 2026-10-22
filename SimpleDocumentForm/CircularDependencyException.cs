namespace SimpleDocumentForm;

/// <summary>
/// Thrown when the calculated fields of a form depend on each other in a cycle.
/// </summary>
public sealed class CircularDependencyException : Exception
{
    public CircularDependencyException(IReadOnlyList<string> cycle)
        : base($"Circular dependency detected: {string.Join(" -> ", cycle)}.")
    {
        Cycle = cycle;
    }

    /// <summary>
    /// The field names forming the cycle, starting and ending with the same field.
    /// </summary>
    public IReadOnlyList<string> Cycle { get; }
}
