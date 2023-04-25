using System.ComponentModel;

namespace elasticsearchApi.Models.Contracts
{
    public interface IPersonData
    {
        [Description("IIN")]
        string? iin { get; set; }
        [Description("Last_Name")]
        string? last_name { get; set; }
        [Description("First_Name")]
        string? first_name { get; set; }
        [Description("Middle_Name")]
        string? middle_name { get; set; }
        [Description("Date_of_Birth")]
        DateTime? date_of_birth { get; set; }
        [Description("Sex")]
        Guid? sex { get; set; }
    }
}
