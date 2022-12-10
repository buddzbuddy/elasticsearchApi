using System;
using System.Collections.Generic;

namespace elasticsearchApi.Models
{
    public class documentDTO
    {
        public Guid id { get; set; }
        public IEnumerable<attributeDTO> attributes { get; set; }
    }
}