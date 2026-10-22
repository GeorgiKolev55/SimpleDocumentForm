namespace SimpleDocumentForm.Api.Forms;

public interface IFormRepository
{
    IReadOnlyList<RegisteredForm> GetAll();

    RegisteredForm? GetById(int id);
}
