using elasticsearchApi.Models.Filters;
using System.ComponentModel;

namespace elasticsearchApi.Models
{
    public enum Genders
    {
        [EnumValue("74C6C7FE-53C6-4492-A62F-65A7A49AB644", "Мужчина")]
        MALE,
        [EnumValue("56E07640-5B5B-47FA-832D-A6639F36EB71", "Женщина")]
        FEMALE
    }

    public enum FamilyStates
    {
        [EnumValue("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}", "Женат/Замужем")]
        MARRIED,
        [EnumValue("{3C58D432-147C-491A-A34A-4A88B2CCBCB5}", "Холост/Не замужем")]
        SINGLE,
        [EnumValue("{2900D318-9207-4241-9CD0-A0B6D6DBC75F}", "Вдова/Вдовец")]
        WIDOW,
        [EnumValue("{EF783D94-0418-4ABA-B653-6DB2A10E4B92}", "Разведен/Разведена")]
        DIVORCED
    }

    public enum PassportTypes
    {
        [EnumValue("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}", "Паспорт")]
        PASSPORT,
        [EnumValue("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}", "Свидетельство о рождении")]
        BIRTHCERTIFICATE
    }
}
