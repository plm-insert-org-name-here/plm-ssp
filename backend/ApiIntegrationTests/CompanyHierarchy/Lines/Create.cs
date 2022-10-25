using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using Xunit;

namespace ApiIntegrationTests.CompanyHierarchy.Lines;

public class Create
{
    public class CanCreateWithUniqueName : IClassFixture<Setup>
    {
        private readonly IRepository<OPU> OPURepo;

        public CanCreateWithUniqueName(Setup setup)
        {
            var scope = setup.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            OPURepo = scope.ServiceProvider.GetRequiredService<IRepository<OPU>>();
        }

        [Fact]
        public async Task Run()
        {
            const int opuId = 1;
            var opu = await OPURepo.FirstOrDefaultAsync(new CHNodeWithChildrenSpec<OPU, Line>(opuId));
            Assert.NotNull(opu);
            Assert.Equal(2, opu.Children.Count);
        }
    }
}