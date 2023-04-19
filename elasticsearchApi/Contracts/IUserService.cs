using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts
{
    public interface IUserService
    {
        Task<List<MyUserDTO>> GetAllMyUsers();
    }
}
