using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FakeItEasy;
using FluentAssertions;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Tests.Entities.CompanyHierarchy
{
    public class OPUTests
    {
        [Fact]
        public void OPU_Rename_ReturnResultOK()
        {
            //Arrange
            var opu = A.Fake<OPU>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Site ,OPU>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(opu.Parent, newName, opu)).Returns(false);
            //Act
            var result = opu.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeTrue();
            opu.Name.Should().Be(newName);
        }
        [Fact]
        public void OPU_Rename_ReturnResultFail()
        {
            //Arrange
            var opu = A.Fake<OPU>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Site, OPU>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(opu.Parent, newName, opu)).Returns(true);
            //Act
            var result = opu.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
    }
}
