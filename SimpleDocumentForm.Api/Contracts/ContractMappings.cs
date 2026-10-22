using SimpleDocumentForm.Api.Forms;
using SimpleDocumentForm.Models;

namespace SimpleDocumentForm.Api.Contracts;

internal static class ContractMappings
{
    public static FormSummaryResponse ToSummaryResponse(this RegisteredForm registeredForm) =>
        new(registeredForm.Id, registeredForm.Name, registeredForm.ToStatus(), registeredForm.Error?.Message);

    public static FormResponse ToResponse(this RegisteredForm registeredForm) =>
        new(
            registeredForm.Id,
            registeredForm.Name,
            registeredForm.ToStatus(),
            registeredForm.Fields.Select(ToResponse).ToList(),
            registeredForm.Error?.Message,
            registeredForm.Error?.Cycle);

    private static FormStatus ToStatus(this RegisteredForm registeredForm) =>
        registeredForm.IsValid ? FormStatus.Valid : FormStatus.Invalid;

    public static FieldType ToFieldType(this FormField field) => field switch
    {
        UserField => FieldType.User,
        CalculatedField => FieldType.Calculated,
        _ => throw new NotSupportedException($"Unsupported field type '{field.GetType().Name}'."),
    };

    private static FormFieldResponse ToResponse(FormField field) => field switch
    {
        CalculatedField calculated => new(
            calculated.Name,
            FieldType.Calculated,
            [calculated.LeftOperand, calculated.RightOperand]),
        _ => new(field.Name, field.ToFieldType()),
    };
}
