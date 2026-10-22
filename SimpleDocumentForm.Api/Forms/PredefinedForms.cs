using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Api.Forms;

public static class PredefinedForms
{
    public static IReadOnlyList<FormDefinition> All { get; } =
    [
        // A = B + C, D = E + F
        new FormDefinition(1, "Form 1",
        [
            new CalculatedField("A", "B", "C"),
            new UserField("B"),
            new UserField("C"),
            new CalculatedField("D", "E", "F"),
            new UserField("E"),
            new UserField("F"),
        ]),

        // A = B + C, C = D + E
        new FormDefinition(2, "Form 2",
        [
            new CalculatedField("A", "B", "C"),
            new UserField("B"),
            new CalculatedField("C", "D", "E"),
            new UserField("D"),
            new UserField("E"),
        ]),

        // A = B + C, C = A + D (circular dependency, rejected at startup)
        new FormDefinition(3, "Form 3",
        [
            new CalculatedField("A", "B", "C"),
            new UserField("B"),
            new CalculatedField("C", "A", "D"),
            new UserField("D"),
        ]),
    ];
}
