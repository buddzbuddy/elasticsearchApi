using elasticsearchApi.Config;
using elasticsearchApi.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace elasticsearchApi.Services
{
    public interface IUserService
    {
        Task<List<MyUserDTO>> GetAllMyUsers();
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly UsersApiOptions _apiOptions;

        public UserService(
            HttpClient httpClient,
            IOptions<UsersApiOptions> options
            ) {
            _httpClient = httpClient;
            _apiOptions = options.Value;
        }

        public async Task<List<MyUserDTO>> GetAllMyUsers()
        {
            var userResponse = await _httpClient.GetAsync(_apiOptions.Endpoint);
            if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<MyUserDTO>();
            var responseContent = userResponse.Content;
            var allUsers = await responseContent.ReadFromJsonAsync<List<MyUserDTO>>();
            return allUsers;
        }
    }
}
