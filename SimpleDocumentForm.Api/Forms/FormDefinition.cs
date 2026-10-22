using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Api.Forms;

/// <summary>
/// Raw definition of a predefined form. It is validated when turned into a <see cref="DocumentForm"/>.
/// </summary>
public sealed record FormDefinition(int Id, string Name, IReadOnlyList<FormField> Fields);
