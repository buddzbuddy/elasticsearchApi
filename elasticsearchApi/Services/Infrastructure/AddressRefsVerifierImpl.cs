using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Data.Entities;
using elasticsearchApi.Models.Exceptions.Base;
using Nest;

namespace elasticsearchApi.Services.Infrastructure
{
    public class AddressRefsVerifierImpl : IAddressRefsVerifier
    {
        private readonly ICacheService _cache;
        public AddressRefsVerifierImpl(ICacheService cache)
        {
            _cache = cache;
        }

        public void Verify(int regionNo, int districtNo)
        {
            var addressRefs = _cache.GetObject(CacheKeys.ADDRESS_REFS_KEY);
            if (addressRefs == null || (addressRefs as AddressEntity[])?.Length == 0)
            {
                throw new AddressRefsException("Адресный справочник не загружен в память! Обратитесь к администраторам системы!");
            }

            if (!((AddressEntity[])addressRefs).Any(x => x.regionNo == regionNo && x.districtNo == districtNo))
            {
                throw new AddressRefsException(string.Format("Номера области и района отсутствуют в справочнике: regionNo - {0}, districtNo - {1}", regionNo, districtNo));
            }
        }
    }
}
