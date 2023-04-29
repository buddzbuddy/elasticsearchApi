using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Helpers
{
    public class NameRandomizer
    {
        private const string WordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumberChars = "0123456789";
        private Dictionary<GeneratorType, string> generatorTypeDic = new Dictionary<GeneratorType, string>
        {
            { GeneratorType.WORD, WordChars }, { GeneratorType.NUMBER, NumberChars }
        };
        private readonly Random Random = new ();
        private readonly int _wordLength;
        public NameRandomizer(int wordLength = 20)
        {
            _wordLength = wordLength;
        }

        public List<string> Generate(GeneratorType generatorType, int batchSize)
        {
            var names = new List<string>(batchSize);
            for (var i = 0; i < batchSize; i++)
            {
                names.Add(RandomString(_wordLength, generatorTypeDic[generatorType]));
            }
            return names;
        }

        private string RandomString(int length, string Chars)
        {
            return new string(Enumerable.Repeat(Chars, length)
                .Select(s => s[Random.Next(s.Length)])
                .ToArray());
        }

        public enum GeneratorType
        {
            WORD, NUMBER
        }
    }
}
