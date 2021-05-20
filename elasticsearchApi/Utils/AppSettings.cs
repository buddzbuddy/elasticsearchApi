using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Utils
{
    public class AppSettings
    {
        public string host { get; set; }
        public string logpath { get; set; }
        public string asist_persons_index_name { get; set; }
        public string nrsz_persons_index_name { get; set; }
    }
}
