using System;
using System.Collections.Generic;

namespace elasticsearchApi.Models
{
    public class documentDTO
    {
        public int? id { get; set; }
        public IEnumerable<attributeDTO> attributes { get; set; }
    }
}