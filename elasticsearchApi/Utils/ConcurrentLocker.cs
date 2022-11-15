using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace elasticsearchApi.Utils
{
    public class ConcurrentLocker
    {
        public readonly ConcurrentDictionary<string, Lazy<SemaphoreSlim>> _dictionary = new();

        public Lazy<SemaphoreSlim> this[string name] => _dictionary.GetOrAdd(name, _ => new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1)));
    }
}
