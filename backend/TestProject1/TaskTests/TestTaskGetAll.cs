using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Features.Tasks;
using LanguageExt;
using Moq;
using Xunit;

namespace TestProject1
{
    public class TestTaskGetAll
    {
        private readonly Mock<ITaskGetAll> getAll;

        public TestTaskGetAll()
        {
            getAll = new Mock<ITaskGetAll>();
        }

        [Fact]
        public async Task GetAllAsync_Test_One_Object()
        {
            getAll.Setup(g => g.HandleAsync(new())).ReturnsAsync(new List<GetAll.Res>()
            {
                new GetAll.Res(0, "test", true)
                {
                    Id = 0,
                    Name = "test",
                    Ordered = true
                }
            });

            var result = await getAll.Object.HandleAsync(new());
            Assert.True(result.Count == 1);
        }
        
        [Fact]
        public async Task GetAllAsync_Test_Multiply_Objects()
        {
            getAll.Setup(g => g.HandleAsync(new())).ReturnsAsync(new List<GetAll.Res>()
            {
                new GetAll.Res(0, "test0", true)
                {
                    Id = 0,
                    Name = "test0",
                    Ordered = true
                },
                new GetAll.Res(1, "test1", true)
                {
                    Id = 1,
                    Name = "test1",
                    Ordered = true
                },
                new GetAll.Res(2, "test2", true)
                {
                    Id = 2,
                    Name = "test2",
                    Ordered = true
                },
                new GetAll.Res(3, "test3", true)
                {
                    Id = 3,
                    Name = "test3",
                    Ordered = true
                }
            });

            var result = await getAll.Object.HandleAsync(new());
            Assert.True(result.Count == 4);
        }
        
    }   
}