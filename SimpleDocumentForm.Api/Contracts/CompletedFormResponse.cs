namespace SimpleDocumentForm.Api.Contracts;

/// <summary>
/// A form with the values of all its fields, based on the user's input.
/// </summary>
public sealed record CompletedFormResponse(int Id, string Name, IReadOnlyList<CompletedFieldResponse> Fields);

public sealed record CompletedFieldResponse(string Name, FieldType Type, decimal Value);
