namespace SimpleDocumentForm.Api.Contracts;

/// <summary>
/// Describes a form and the fields the user has to fill in. <see cref="Error"/> and <see cref="Cycle"/>
/// are only present for invalid forms; <see cref="Cycle"/> lists the fields of a circular dependency.
/// </summary>
public sealed record FormResponse(
    int Id,
    string Name,
    FormStatus Status,
    IReadOnlyList<FormFieldResponse> Fields,
    string? Error = null,
    IReadOnlyList<string>? Cycle = null);

/// <summary>
/// A field on a form. <see cref="Operands"/> is only present for calculated fields.
/// </summary>
public sealed record FormFieldResponse(string Name, FieldType Type, IReadOnlyList<string>? Operands = null);
