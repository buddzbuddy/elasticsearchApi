using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Last_Name { get; set; }
        public string First_Name { get; set; }
        public string Middle_Name { get; set; }
        public string IIN { get; set; }
        public string SIN { get; set; }
        public Guid? Sex { get; set; }
        public DateTime? Date_of_Birth { get; set; }
        public Guid? PassportType { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNo { get; set; }
        public DateTime? Date_of_Issue { get; set; }
        public string Issuing_Authority { get; set; }
        public Guid? FamilyState { get; set; }
    }
    public class SearchPersonModel
    {
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string iin { get; set; }
        public Guid? sex { get; set; }
        public string date_of_birth { get; set; }
        public Guid? passporttype { get; set; }
        public string passportseries { get; set; }
        public string passportno { get; set; }
        public string date_of_issue { get; set; }
        public string issuing_authority { get; set; }
        public Guid? familydtate { get; set; }
    }

    public class personDTO
    {
        public Guid id { get; set; }
        [Description("IIN")]
        public string iin { get; set; }
        [Description("Last_Name")]
        public string last_name { get; set; }
        [Description("First_Name")]
        public string first_name { get; set; }
        [Description("Middle_Name")]
        public string middle_name { get; set; }
        [Description("Date_of_Birth")]
        public DateTime? date_of_birth { get; set; }
        [Description("Sex")]
        public Guid? sex { get; set; }
        [Description("PassportType")]
        public Guid? passporttype { get; set; }
        [Description("PassportSeries")]
        public string passportseries { get; set; }
        [Description("PassportNo")]
        public string passportno { get; set; }
        [Description("Date_of_Issue")]
        public DateTime? date_of_issue { get; set; }
        [Description("Issuing_Authority")]
        public string issuing_authority { get; set; }
        [Description("FamilyState")]
        public Guid? familystate { get; set; }
    }
}
