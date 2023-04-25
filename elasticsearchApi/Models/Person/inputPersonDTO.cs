using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Filters;
using elasticsearchApi.Models.Passport;
using System.ComponentModel;

namespace elasticsearchApi.Models.Person
{
    public class inputPersonDTO: PersonFullData, IBaseEntity
    {
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
