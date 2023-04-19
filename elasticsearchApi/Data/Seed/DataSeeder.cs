using elasticsearchApi.Data.Entities;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace elasticsearchApi.Data.Seed
{
    public static class DataSeeder
    {
        public static List<AddressEntity> addressEntities = new List<AddressEntity>(JsonConvert.DeserializeObject<AddressEntity[]>(address_json) ?? Array.Empty<AddressEntity>());

        const string address_json = @"
[
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 54,
    ""districtName"": ""н.Рушон""
  },
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 55,
    ""districtName"": ""н.Шугнон""
  },
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 52,
    ""districtName"": ""н.Ишкошим""
  },
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 50,
    ""districtName"": ""н.Ванч""
  },
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 51,
    ""districtName"": ""н.Мургаб""
  },
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 48,
    ""districtName"": ""ш.Хорог""
  },
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 49,
    ""districtName"": ""н.Дарваз""
  },
  {
    ""regionNo"": 4,
    ""regionName"": ""ГБАО"",
    ""districtNo"": 53,
    ""districtName"": ""н.Рошткалъа""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 74,
    ""districtName"": ""ш.Турсунзода""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 66,
    ""districtName"": ""н.Точикобод""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 63,
    ""districtName"": ""н.Лахш""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 68,
    ""districtName"": ""ш.Хисор""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 57,
    ""districtName"": ""ш.Вахдат""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 58,
    ""districtName"": ""н.Рудаки""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 56,
    ""districtName"": ""н.Шахринав""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 61,
    ""districtName"": ""н.Нурабад""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 64,
    ""districtName"": ""н.Файзобод""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 60,
    ""districtName"": ""н.Варзоб""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 65,
    ""districtName"": ""н.Сангвор""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 67,
    ""districtName"": ""н.Рашт""
  },
  {
    ""regionNo"": 5,
    ""regionName"": ""РРП"",
    ""districtNo"": 62,
    ""districtName"": ""ш.Рогун""
  },
  {
    ""regionNo"": 1,
    ""regionName"": ""Душанбе"",
    ""districtNo"": 2,
    ""districtName"": ""н.Шохмансур (ш.Душанбе)""
  },
  {
    ""regionNo"": 1,
    ""regionName"": ""Душанбе"",
    ""districtNo"": 1,
    ""districtName"": ""н.Фирдавси (ш.Душанбе)""
  },
  {
    ""regionNo"": 1,
    ""regionName"": ""Душанбе"",
    ""districtNo"": 4,
    ""districtName"": ""н.И.Сомони (ш.Душанбе)""
  },
  {
    ""regionNo"": 1,
    ""regionName"": ""Душанбе"",
    ""districtNo"": 3,
    ""districtName"": ""н.Сино (ш.Душанбе)""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 44,
    ""districtName"": ""ш.Бохтар""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 41,
    ""districtName"": ""н.Муъминобод""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 45,
    ""districtName"": ""н.Ёвон""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 46,
    ""districtName"": ""н.Фархор""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 40,
    ""districtName"": ""н.Ховалинг""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 84,
    ""districtName"": ""н.Кушониён""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 34,
    ""districtName"": ""н.Дусти""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 24,
    ""districtName"": ""н.Восеъ""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 25,
    ""districtName"": ""н.Хамадони""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 31,
    ""districtName"": ""н.Вахш""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 38,
    ""districtName"": ""ш.Леваканд""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 30,
    ""districtName"": ""н.Ч.Балхи""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 27,
    ""districtName"": ""н.Темурмалик""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 23,
    ""districtName"": ""ш.Куляб""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 29,
    ""districtName"": ""н.Шахритус""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 32,
    ""districtName"": ""н.Чайхун""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 47,
    ""districtName"": ""ш.Норак""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 39,
    ""districtName"": ""н.Панч""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 42,
    ""districtName"": ""н.Шамсиддин Шохин""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 43,
    ""districtName"": ""н.Бальджувон""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 37,
    ""districtName"": ""н.Н.Хусрав""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 28,
    ""districtName"": ""н.А.Чоми""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 33,
    ""districtName"": ""н.Кубодиён""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 26,
    ""districtName"": ""н.Дангара""
  },
  {
    ""regionNo"": 3,
    ""regionName"": ""Хатлон"",
    ""districtNo"": 36,
    ""districtName"": ""н.Хуросон""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 18,
    ""districtName"": ""ш.Панчакент""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 21,
    ""districtName"": ""ш.Худжанд""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 17,
    ""districtName"": ""ш.Истаравшан""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 8,
    ""districtName"": ""ш.Исфара""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 15,
    ""districtName"": ""ш.Бустон""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 7,
    ""districtName"": ""н.Зафарабад""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 16,
    ""districtName"": ""н.Шахристан""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 5,
    ""districtName"": ""н.Ашт""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 76,
    ""districtName"": ""н.Деваштич""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 10,
    ""districtName"": ""н.Мастчох""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 22,
    ""districtName"": ""н.Кухистони Мастчох""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 14,
    ""districtName"": ""ш.Истиклол""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 20,
    ""districtName"": ""ш.Конибодом""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 19,
    ""districtName"": ""н.Айни""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 9,
    ""districtName"": ""ш.Гулистон""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 12,
    ""districtName"": ""н.Чаббор Расулов""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 13,
    ""districtName"": ""н.Бобочон Гафуров""
  },
  {
    ""regionNo"": 2,
    ""regionName"": ""Согд"",
    ""districtNo"": 11,
    ""districtName"": ""н.Спитамен""
  }
]
";
    }
}
