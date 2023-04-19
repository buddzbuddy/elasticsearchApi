namespace elasticsearchApi.Data.Entities
{
    public class PassportEntity
    {
        public int PersonId { get; set; }
        public Guid? PassportType { get; set; }
        public string? PassportSeries { get; set;}
        public string? PassportNo { get; set; }
        public DateTime? Date_of_Issue { get; set; }
        public string? Issuing_Authority { get; set; }
        public Guid? Marital_Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
