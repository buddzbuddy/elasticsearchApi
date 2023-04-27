namespace elasticsearchApi.Models.Filters
{
    public class EnumValueAttribute : Attribute
    {
        public Guid ValueId { get; private set; }
        public string Text { get; private set; }

        public EnumValueAttribute(string id, string text)
        {
            ValueId = Guid.Parse(id);
            Text = text;
        }
    }
}
