using System;

namespace elasticsearchApi.Models
{
    public class attributeDTO
    {
        //public Guid id { get; set; }
        public string? name { get; set; }
        /*public string caption { get; set; }
        public string type { get; set; }
        public Guid enumDef { get; set; }
        public Guid guidId { get; set; }
        public Guid docDef { get; set; }*/
        public object? value { get; set; }
        /*public string enumValueText { get; set; }
        public documentDTO subDocument { get; set; }*/
    }
}