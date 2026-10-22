using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Tests;

public class DocumentFormTests
{
    [Fact]
    public void Constructor_Form1_CreatesFormWithAllFields()
    {
        var form = SampleForms.Form1();

        Assert.Equal(["A", "B", "C", "D", "E", "F"], form.Fields.Select(f => f.Name));
        Assert.Equal(4, form.Fields.OfType<UserField>().Count());
        Assert.Equal(2, form.Fields.OfType<CalculatedField>().Count());
    }

    [Fact]
    public void Constructor_Form2_CreatesFormWithAllFields()
    {
        var form = SampleForms.Form2();

        Assert.Equal(["A", "B", "C", "D", "E"], form.Fields.Select(f => f.Name));
        Assert.Equal(3, form.Fields.OfType<UserField>().Count());
        Assert.Equal(2, form.Fields.OfType<CalculatedField>().Count());
    }

    [Fact]
    public void Constructor_Form3_ThrowsCircularDependencyException()
    {
        var exception = Assert.Throws<CircularDependencyException>(
            () => new DocumentForm("Form 3", SampleForms.Form3Fields()));

        Assert.Equal(["A", "C", "A"], exception.Cycle);
    }

    [Fact]
    public void Constructor_SelfReference_ThrowsCircularDependencyException()
    {
        var exception = Assert.Throws<CircularDependencyException>(
            () => new DocumentForm("Self", [new CalculatedField("A", "A", "B"), new UserField("B")]));

        Assert.Equal(["A", "A"], exception.Cycle);
    }

    [Fact]
    public void Constructor_IndirectCycle_ThrowsCircularDependencyException()
    {
        var exception = Assert.Throws<CircularDependencyException>(() => new DocumentForm("Indirect",
        [
            new CalculatedField("A", "B", "X"),
            new CalculatedField("B", "C", "X"),
            new CalculatedField("C", "A", "X"),
            new UserField("X"),
        ]));

        Assert.Equal(["A", "B", "C", "A"], exception.Cycle);
    }

    [Fact]
    public void Constructor_SharedDependency_IsNotTreatedAsCycle()
    {
        var form = new DocumentForm("Diamond",
        [
            new CalculatedField("A", "B", "C"),
            new CalculatedField("B", "D", "E"),
            new CalculatedField("C", "D", "E"),
            new UserField("D"),
            new UserField("E"),
        ]);

        Assert.Equal(5, form.Fields.Count);
    }

    [Fact]
    public void Constructor_UnknownOperand_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DocumentForm("Unknown", [new CalculatedField("A", "B", "Z"), new UserField("B")]));

        Assert.Contains("'Z'", exception.Message);
    }

    [Fact]
    public void Constructor_DuplicateFieldName_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DocumentForm("Duplicate", [new UserField("A"), new UserField("A")]));

        Assert.Contains("'A'", exception.Message);
    }

    [Fact]
    public void Constructor_NullField_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new DocumentForm("Null", [new UserField("A"), null!]));
    }

    [Fact]
    public void TryGetField_ExistingField_ReturnsField()
    {
        var form = SampleForms.Form2();

        var found = form.TryGetField("C", out var field);

        Assert.True(found);
        Assert.Equal(new CalculatedField("C", "D", "E"), field);
    }

    [Fact]
    public void TryGetField_UnknownField_ReturnsFalse()
    {
        var form = SampleForms.Form2();

        Assert.False(form.TryGetField("Z", out _));
    }
}
