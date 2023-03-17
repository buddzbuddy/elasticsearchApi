using System.ComponentModel.DataAnnotations;

namespace elasticsearchApi.Models
{
    public class AddUserModel
    {
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
    }
}
