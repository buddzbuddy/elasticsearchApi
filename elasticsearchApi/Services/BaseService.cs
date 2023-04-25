using elasticsearchApi.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using elasticsearchApi.Utils;

namespace elasticsearchApi.Services
{
    public abstract class BaseService
    {
        public static IDictionary<string, object> ModelToDict<T>(T obj)
        {
            Dictionary<string, object> dict = new();
            foreach (var propInfo in typeof(T).GetFilteredProperties())
            {
                var field_name = propInfo.Name.ToLower();
                var fieldVal = propInfo.GetValue(obj);
                if (/*field_name != "id" && */fieldVal != null && !string.IsNullOrEmpty(fieldVal.ToString()))
                    dict.Add(field_name, fieldVal);
            }
            return dict;
        }
        public static documentDTO ModelToDocumentWithAttributes<T>(T obj)
        {
            var res = new documentDTO
            {
                attributes = Array.Empty<attributeDTO>()
            };
            foreach (var propInfo in typeof(T).GetProperties(System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                var v = propInfo.GetValue(obj);
                if (propInfo.Name == "id" || v.IsNullOrEmpty()) continue;
                if (propInfo.Name == "id" && v != null && v is int id && id > 0)
                {
                    res.id = id;
                }
                else
                {
                    var attr = new attributeDTO { name = (propInfo.GetCustomAttributes(true)[0] as DescriptionAttribute)?.Description, value = v };
                    res.attributes = res.attributes.Append(attr);
                }
            }
            return res;
        }
        public static bool DocumentToDict(documentDTO obj, out IDictionary<string, object?> dict, out string[] errorMessages)
        {
            errorMessages = Array.Empty<string>();
            dict = new Dictionary<string, object?>();
            if (obj != null)
            {
                if (obj.id != null) dict["id"] = obj.id;
                if(obj.attributes != null)
                foreach (var attr in obj.attributes)
                {
                    if (attr.value.IsNullOrEmpty() || attr.name.IsNullOrEmpty()) continue;
                    var attrname = attr?.name?.ToLower();
                    if (!dict.ContainsKey(attrname))
                    {
                        dict[attrname] = attr?.value?.ToString();
                    }
                    else
                    {
                        errorMessages = errorMessages.Append($"Поле {attr?.name} дублируется в теле запроса! Дубликаты полей недопустимы!").ToArray();
                    }
                }
            }
            return errorMessages.Length == 0;
        }
        private static readonly SemaphoreSlim semaphoreLog = new(1, 1);
        public static void WriteLog(string text, string pathToFile)
        {
            semaphoreLog.Wait();
            //lock (lockObj)
            {
                using (StreamWriter sw = new StreamWriter(pathToFile, true))
                {
                    sw.WriteLine(text);
                }
            }
            semaphoreLog.Release();
        }
    }
}
