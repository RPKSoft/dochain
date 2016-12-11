using System.Threading.Tasks;

namespace Dochain.Web.Interfaces
{
    public interface IDochainService
    {
        Task Add(string name, string value);

        Task Add(string name, byte[] value);

        Task<bool> IsAvailable(string name);

        Task<bool> IsValid(string name, string value);

        Task<bool> IsValid(string name, byte[] value);

        Task<Document> GetDocumentInfo(string name);
    }
}