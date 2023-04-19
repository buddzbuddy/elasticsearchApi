using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Utils
{
    public class AppSettings
    {
        public string? es_host { get; set; }
        public string? logpath { get; set; }
        public string? error_logpath { get; set; }
        public bool? log_enabled { get; set; }
        public string? asist_persons_index_name { get; set; }
        public string? nrsz_persons_index_name { get; set; }
        public string? asist_data_connection { get; set; }
        public string? asist_meta_connection { get; set; }
        public string? nrsz_connection { get; set; }
        public string? elasticUser { get; set; }
        public string? elasticPass { get; set; }
    }
}
