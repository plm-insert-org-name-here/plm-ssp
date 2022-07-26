using System.Threading;
using System.Threading.Tasks;
using Api.Features.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Task = Api.Domain.Entities.Task;

namespace TestProject1
{
    public class TestTaskCreate
    {
        private readonly Mock<ITaskCreate> create;

        public TestTaskCreate()
        {
            create = new Mock<ITaskCreate>();
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Test()
        {
            Create.Req req = null;
            create.Setup(c => c._HandleAsync(It.IsAny<Create.Req>()))
                .Callback<Create.Req>(x => req = x);
            var reqData = new Create.Req("test", true, 0)
            {
                Name = "test",
                Ordered = true,
                JobId = 0
            };

            await create.Object._HandleAsync(reqData);
            
            create.Verify(x => x._HandleAsync(It.IsAny<Create.Req>()), Times.Once);
            Assert.Equal(req.Name, reqData.Name);
            Assert.Equal(req.Ordered, reqData.Ordered);
            Assert.Equal(req.JobId, reqData.JobId);
        }
    }
}