using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Filters;
using elasticsearchApi.Models.Passport;
using System.ComponentModel;

namespace elasticsearchApi.Models.Person
{
    public class inputPersonDTO: IPersonData, IPassportData, IBaseEntity
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

        [SkipProperty]
        public object? this[string key]
        {
            get
            {
                var propInfo = GetType().GetProperty(key);
                if (propInfo != null)
                    return propInfo.GetValue(this);
                else
                    throw new ArgumentException($"Поле {key} не существует!");
            }
            set
            {
                var propInfo = GetType().GetProperty(key);
                if (propInfo != null)
                    propInfo.SetValue(this, value);
                else
                    throw new ArgumentException($"Поле {key} не существует!");
            }
        }

        public bool Equals(IDictionary<string, object?> filter)
        {
            bool equals = false;
            foreach (var filterField in filter)
            {
                var val = this[filterField.Key];
                if (filterField.Value != null && val != null)
                {
                    equals = val.Equals(filterField.Value);
                }
            }
            return equals;
        }
    }
}
