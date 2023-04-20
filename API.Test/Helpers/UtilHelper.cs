using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Helpers
{
    public static class UtilHelper
    {
        public static IDictionary<string, object?> ConvertToDictionary<T>(T obj)
        {
            var model = new Dictionary<string, object?>();

            foreach (var propInfo in typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance))
            {
                model.Add(propInfo.Name, propInfo.GetValue(obj));
            }

            return model;
        }
    }
}
