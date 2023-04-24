namespace elasticsearchApi.Models.Passport
{
    public class modifyPersonPassportDTO
    {
        public Guid? passporttype { get; set; }
        public string? passportseries { get; set; }
        public string? passportno { get; set; }
        public DateTime? date_of_issue { get; set; }
        public string? issuing_authority { get; set; }
        public Guid? familystate { get; set; }
    }
}
