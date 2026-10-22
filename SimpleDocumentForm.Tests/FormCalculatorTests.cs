using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Tests;

public class FormCalculatorTests
{
    private static readonly Dictionary<string, decimal> Form1Values = new()
    {
        ["B"] = 1,
        ["C"] = 2,
        ["E"] = 3,
        ["F"] = 4,
    };

    private static readonly Dictionary<string, decimal> Form2Values = new()
    {
        ["B"] = 1,
        ["D"] = 2,
        ["E"] = 3,
    };

    [Theory]
    [InlineData("A", 3)]
    [InlineData("D", 7)]
    public void Form1_CalculatedField_ReturnsSumOfOperands(string fieldName, int expected)
    {
        var result = FormCalculator.CalculateFieldValue(SampleForms.Form1(), Form1Values, fieldName);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("B", 1)]
    [InlineData("C", 2)]
    [InlineData("E", 3)]
    [InlineData("F", 4)]
    public void Form1_UserField_ReturnsEnteredValue(string fieldName, int expected)
    {
        var result = FormCalculator.CalculateFieldValue(SampleForms.Form1(), Form1Values, fieldName);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C", 5)]
    [InlineData("A", 6)]
    public void Form2_NestedCalculatedField_ReturnsSumOfResolvedOperands(string fieldName, int expected)
    {
        var result = FormCalculator.CalculateFieldValue(SampleForms.Form2(), Form2Values, fieldName);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Form2_FractionalAndNegativeValues_AreSummedExactly()
    {
        var values = new Dictionary<string, decimal> { ["B"] = 1.5m, ["D"] = -2.25m, ["E"] = 10.1m };

        var result = FormCalculator.CalculateFieldValue(SampleForms.Form2(), values, "A");

        Assert.Equal(9.35m, result);
    }

    [Fact]
    public void CalculateFieldValue_OnlyRequiresValuesOfDependencies()
    {
        var values = new Dictionary<string, decimal> { ["B"] = 10, ["C"] = 20 };

        var result = FormCalculator.CalculateFieldValue(SampleForms.Form1(), values, "A");

        Assert.Equal(30, result);
    }

    [Fact]
    public void CalculateFieldValue_SameOperandUsedTwice_ReturnsDoubleValue()
    {
        var form = new DocumentForm("Double", [new UserField("B"), new CalculatedField("A", "B", "B")]);
        var values = new Dictionary<string, decimal> { ["B"] = 21 };

        var result = FormCalculator.CalculateFieldValue(form, values, "A");

        Assert.Equal(42, result);
    }

    [Fact]
    public void CalculateFieldValue_SharedDependency_ReturnsCorrectValue()
    {
        var form = new DocumentForm("Diamond",
        [
            new CalculatedField("A", "B", "C"),
            new CalculatedField("B", "D", "E"),
            new CalculatedField("C", "D", "E"),
            new UserField("D"),
            new UserField("E"),
        ]);
        var values = new Dictionary<string, decimal> { ["D"] = 2, ["E"] = 3 };

        var result = FormCalculator.CalculateFieldValue(form, values, "A");

        Assert.Equal(10, result);
    }

    [Fact]
    public void CalculateFieldValue_MissingUserValue_ThrowsArgumentException()
    {
        var values = new Dictionary<string, decimal> { ["B"] = 1 };

        var exception = Assert.Throws<ArgumentException>(
            () => FormCalculator.CalculateFieldValue(SampleForms.Form1(), values, "A"));

        Assert.Equal("userValues", exception.ParamName);
        Assert.Contains("'C'", exception.Message);
    }

    [Fact]
    public void CalculateFieldValue_UnknownField_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => FormCalculator.CalculateFieldValue(SampleForms.Form1(), Form1Values, "Z"));

        Assert.Equal("fieldName", exception.ParamName);
    }

    [Fact]
    public void CalculateFieldValue_NullForm_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => FormCalculator.CalculateFieldValue(null!, Form1Values, "A"));
    }

    [Fact]
    public void CalculateFieldValue_NullUserValues_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => FormCalculator.CalculateFieldValue(SampleForms.Form1(), null!, "A"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CalculateFieldValue_EmptyFieldName_ThrowsArgumentException(string fieldName)
    {
        Assert.Throws<ArgumentException>(
            () => FormCalculator.CalculateFieldValue(SampleForms.Form1(), Form1Values, fieldName));
    }
}
