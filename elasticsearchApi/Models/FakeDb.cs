using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Models
{
    public static class FakeDb
    {
        public static IDictionary<int, int> RegCounters { get; set; }
        public static void Increase(int regCode)
        {
            if (RegCounters.ContainsKey(regCode)) RegCounters[regCode]++;
            else throw new ApplicationException($"Код района {regCode} для счеткчика не найден: Increase() -> RegCounters");
        }
    }
}
