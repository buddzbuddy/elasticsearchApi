using System.ComponentModel;

namespace elasticsearchApi.Models.Contracts
{
    public interface IPassportData
    {
        [Description("PassportType")]
        Guid? passporttype { get; set; }
        [Description("PassportSeries")]
        string? passportseries { get; set; }
        [Description("PassportNo")]
        string? passportno { get; set; }
        [Description("Date_of_Issue")]
        DateTime? date_of_issue { get; set; }
        [Description("Issuing_Authority")]
        string? issuing_authority { get; set; }
        [Description("FamilyState")]
        Guid? familystate { get; set; }
    }
}
