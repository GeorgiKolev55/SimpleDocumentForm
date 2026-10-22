namespace SimpleDocumentForm.Models;

/// <summary>
/// Base type for a numeric field on a document form.
/// </summary>
public abstract record FormField
{
    protected FormField(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }
}

/// <summary>
/// A field whose value is entered by the user.
/// </summary>
public sealed record UserField : FormField
{
    public UserField(string name) : base(name)
    {
    }
}

/// <summary>
/// A field whose value is calculated as the sum of two other fields.
/// </summary>
public sealed record CalculatedField : FormField
{
    public CalculatedField(string name, string leftOperand, string rightOperand) : base(name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leftOperand);
        ArgumentException.ThrowIfNullOrWhiteSpace(rightOperand);

        LeftOperand = leftOperand;
        RightOperand = rightOperand;
    }

    public string LeftOperand { get; }

    public string RightOperand { get; }
}
