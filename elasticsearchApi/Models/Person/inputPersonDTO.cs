using System.ComponentModel;

namespace elasticsearchApi.Models.Person
{
    public class inputPersonDTO
    {

        [Description("IIN")]
        public string? iin { get; set; }
        [Description("Last_Name")]
        public string? last_name { get; set; }
        [Description("First_Name")]
        public string? first_name { get; set; }
        [Description("Middle_Name")]
        public string? middle_name { get; set; }
        [Description("Date_of_Birth")]
        public DateTime? date_of_birth { get; set; }
        [Description("Sex")]
        public Guid? sex { get; set; }
        [Description("PassportType")]
        public Guid? passporttype { get; set; }
        [Description("PassportSeries")]
        public string? passportseries { get; set; }
        [Description("PassportNo")]
        public string? passportno { get; set; }
        [Description("Date_of_Issue")]
        public DateTime? date_of_issue { get; set; }
        [Description("Issuing_Authority")]
        public string? issuing_authority { get; set; }
        [Description("FamilyState")]
        public Guid? familystate { get; set; }
    }
}
