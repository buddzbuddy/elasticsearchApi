using System;

namespace elasticsearchApi.Models
{
    public class documentDTO
    {
        public Guid id { get; set; }
        public attributeDTO[] attributes { get; set; } = Array.Empty<attributeDTO>();
    }
}