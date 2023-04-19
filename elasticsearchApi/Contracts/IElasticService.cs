using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts
{
    public interface IElasticService
    {
        void FindSamePersonES(inputPersonDTO inputData, ref IServiceContext context, int page = 1, int size = 10);
        void FindPersonsES(inputPersonDTO inputData, ref IServiceContext context, bool fuzzy = false, int page = 1, int size = 10);
        bool FilterES(IDictionary<string, object> filter, out outPersonDTO[] data, out string[] errorMessages, out long totalCount, bool fuzzy = false, int page = 1, int size = 10);
        bool FilterDocumentES(documentDTO filter, out IEnumerable<documentDTO> data, out string[] errorMessages, bool fuzzy = false, int page = 1, int size = 10);
        void FindPersonByPinES(string iin, ref IServiceContext context, int page = 1, int size = 10);
    }
}
