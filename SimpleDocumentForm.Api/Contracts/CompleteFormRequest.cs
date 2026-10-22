using System.ComponentModel.DataAnnotations;

namespace SimpleDocumentForm.Api.Contracts;

/// <summary>
/// User-entered values keyed by user field name.
/// </summary>
public sealed record CompleteFormRequest([Required] IReadOnlyDictionary<string, decimal> Values);
