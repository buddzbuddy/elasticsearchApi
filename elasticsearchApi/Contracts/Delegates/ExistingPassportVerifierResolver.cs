using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.Passport;

namespace elasticsearchApi.Contracts.Delegates
{
    public delegate ICheckProvider ExistingPassportVerifierResolver(string indentifier);
}
