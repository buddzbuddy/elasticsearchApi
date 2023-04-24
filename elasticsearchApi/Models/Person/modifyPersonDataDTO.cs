namespace elasticsearchApi.Models.Person
{
    public class modifyPersonDataDTO
    {
        public string? last_name { get; set; }
        public string? first_name { get; set; }
        public string? middle_name { get; set; }
        public DateTime? date_of_birth { get; set; }
        public Guid? sex { get; set; }
    }
}
