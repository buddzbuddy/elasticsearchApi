namespace elasticsearchApi.Data.Entities
{
    public class PersonEntity
    {
        public int Id { get; set; }
        public string? IIN { get; set; }
        public string? SIN { get; set; }
        public string? Last_Name { get; set; }
        public string? First_Name { get; set; }
        public string? Middle_Name { get; set; }
        public DateTime? Date_of_Birth { get; set; }
        public Guid? Sex { get; set; }
        
        public Guid? PassportType { get; set; }
        public string? PassportSeries { get; set; }
        public string? PassportNo { get; set; }
        public DateTime? Date_of_Issue { get; set; }
        public string? Issuing_Authority { get; set; }
        public Guid? FamilyState { get; set; }

        // System Computed Fields
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool deleted { get; set; }
    }
}
