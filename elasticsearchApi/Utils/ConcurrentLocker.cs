using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Utils
{
    public class ConcurrentLocker
    {
        readonly ConcurrentDictionary<string, object> _dictionary = new();

        public object this[string name] => _dictionary.GetOrAdd(name, _ => new object());
    }
}
