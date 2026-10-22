namespace SimpleDocumentForm.Api.Contracts;

public enum FormStatus
{
    /// <summary>The form can be filled in and calculated.</summary>
    Valid,

    /// <summary>The form definition is invalid (for example a circular dependency) and cannot be calculated.</summary>
    Invalid,
}
