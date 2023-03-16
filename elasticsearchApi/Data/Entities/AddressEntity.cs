using SqlKata;

namespace elasticsearchApi.Data.Entities
{
    public class AddressEntity
    {
        public int regionNo { get; set; }
        public string? regionName { get; set; }
        public int districtNo { get; set; }
        public string? districtName { get; set; }
    }
}
