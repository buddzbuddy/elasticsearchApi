using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using elasticsearchApi.Models.Filters;
using System.ComponentModel;

namespace elasticsearchApi.Utils
{
    public class StaticReferences
    {
        public static IEnumerable<myEnumItem> getEnumItems<T>() where T : Enum
        {
            var items = (T[])Enum.GetValues(typeof(T));
            return items.Select(x => new myEnumItem { id = x.GetValueId(), text = x.GetValueText() });
        }
        public static bool IsAnyNullOrEmpty(object myObject, out string[] nullFields, params string[] specFieds)
        {
            var list = new List<string>();
            var checkFields = myObject.GetType().GetProperties().AsEnumerable();
            if (specFieds != null && specFieds.Length > 0) checkFields = checkFields.Where(x => specFieds
            .Select(x => x.ToUpper().Trim()).Contains(x.Name.ToUpper().Trim()));
            foreach (PropertyInfo pi in checkFields)
            {
                if (pi.PropertyType == typeof(string))
                {
                    if (string.IsNullOrEmpty(pi.GetValue(myObject) as string))
                    {
                        list.Add(pi.Name);
                    }
                }
                else if (pi.GetValue(myObject) == null) list.Add(pi.Name);
            }
            nullFields = list.ToArray();
            return nullFields.Length > 0;
        }
        
        public static TOutput? InitInhertedProperties<TInput, TOutput>(TInput baseClassInstance)
        {
            TOutput? output = (TOutput?)Activator.CreateInstance(typeof(TOutput));
            foreach (PropertyInfo propertyInfo in typeof(TInput).GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public))
            {
                object? value = propertyInfo.GetValue(baseClassInstance);
                if (value != null) propertyInfo.SetValue(output, value);
            }
            return output;
        }
    }

    public class myEnumItem
    {
        public Guid? id { get; set; }
        public string? text { get; set; }
    }
}
