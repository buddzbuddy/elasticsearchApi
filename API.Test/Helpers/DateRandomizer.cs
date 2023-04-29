using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Helpers
{
    public class DateRandomizer
    {
        private readonly DateTime _start;
        private readonly DateTime _end;
        public DateRandomizer(DateTime start, DateTime end) {
            _start= start;
            _end= end;
        }
        private Random gen = new Random();
        public DateTime Generate()
        {
            int range = (_end - _start).Days;
            return _start.AddDays(gen.Next(range));
        }
    }
}
