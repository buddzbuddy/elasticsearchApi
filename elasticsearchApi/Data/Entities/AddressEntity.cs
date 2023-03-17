using SqlKata;
using System.ComponentModel.DataAnnotations.Schema;

namespace elasticsearchApi.Data.Entities
{
    public class AddressEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int regionNo { get; set; }
        public string? regionName { get; set; }
        public int districtNo { get; set; }
        public string? districtName { get; set; }
    }
}
