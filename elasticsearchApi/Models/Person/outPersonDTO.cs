using elasticsearchApi.Contracts.Infrastructure;

namespace elasticsearchApi.Models.Person
{
    public class outPersonDTO : inputPersonDTO, IPrototype<outPersonDTO>
    {
        public int id { get; set; }

        public outPersonDTO CreateDeepCopy()
        {
            var person = (outPersonDTO)MemberwiseClone();
            return person;
        }
    }
}
