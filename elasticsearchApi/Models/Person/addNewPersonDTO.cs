using elasticsearchApi.Models.Contracts;

namespace elasticsearchApi.Models.Person
{
    public class addNewPersonDTO : IPersonData, IPassportData
    {
        public string? iin { get; set; }
        public string? last_name { get; set; }
        public string? first_name { get; set; }
        public string? middle_name { get; set; }
        public DateTime? date_of_birth { get; set; }
        public Guid? sex { get; set; }
        public Guid? passporttype { get; set; }
        public string? passportseries { get; set; }
        public string? passportno { get; set; }
        public DateTime? date_of_issue { get; set; }
        public string? issuing_authority { get; set; }
        public Guid? familystate { get; set; }

    }
}
