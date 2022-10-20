using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;

namespace Domain.Services;

public class SiteNameUniquenessChecker : ICHNameUniquenessChecker<Site>
{
    private readonly IRepository<Site> _siteRepo;

    public SiteNameUniquenessChecker(IRepository<Site> siteRepo)
    {
        _siteRepo = siteRepo;
    }

    public async Task<bool> Check(string name, Site? site)
    {
        return await _siteRepo.AnyAsync(new SiteNameUniquenessSpec<Site>(name, site));
    }
}