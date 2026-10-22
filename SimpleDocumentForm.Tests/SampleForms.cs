using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Tests;

internal static class SampleForms
{
    // Fields: A, B, C, D, E, F; user fields: B, C, E, F; A = B + C, D = E + F
    public static DocumentForm Form1() => new("Form 1",
    [
        new CalculatedField("A", "B", "C"),
        new UserField("B"),
        new UserField("C"),
        new CalculatedField("D", "E", "F"),
        new UserField("E"),
        new UserField("F"),
    ]);

    // Fields: A, B, C, D, E; user fields: B, D, E; A = B + C, C = D + E
    public static DocumentForm Form2() => new("Form 2",
    [
        new CalculatedField("A", "B", "C"),
        new UserField("B"),
        new CalculatedField("C", "D", "E"),
        new UserField("D"),
        new UserField("E"),
    ]);

    // Fields: A, B, C, D; user fields: B, D; A = B + C, C = A + D
    public static IReadOnlyList<FormField> Form3Fields() =>
    [
        new CalculatedField("A", "B", "C"),
        new UserField("B"),
        new CalculatedField("C", "A", "D"),
        new UserField("D"),
    ];
}
