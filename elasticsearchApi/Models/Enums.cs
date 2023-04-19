using System.ComponentModel;

namespace elasticsearchApi.Models
{
    public enum ModifyPersonPassportResult
    {
        [Description("Success")]
        OK,
        [Description("data entry requirements are not match")]
        INPUT_DATA_ERROR,
        [Description("Гражданин с указанным ПИН не найден!")]
        NRSZ_NOT_FOUND_BY_PIN,
        [Description("same passport found in nrsz db")]
        PASSPORT_DUPLICATE,
        [Description("Архивация старых паспортных данных не записалась в историю")]
        PASSPORT_NOT_INSERTED_TO_ARCHIVE,
        [Description("Паспортные данные гражданина в НРСЗ не обновлены")]
        PASSPORT_NOT_UPDATED
    }
}
