namespace SimpleDocumentForm.Api.Contracts;

/// <summary>
/// A form in the list of predefined forms. <see cref="Error"/> is only present for invalid forms.
/// </summary>
public sealed record FormSummaryResponse(int Id, string Name, FormStatus Status, string? Error = null);
