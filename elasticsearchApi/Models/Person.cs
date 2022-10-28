using System;
using System.Collections.Generic;
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

   
}
