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
    public class SiteTests
    {
        [Fact]
        public void Site_Rename_ReturnResultOK()
        {
            //Arrange
            var site = A.Fake<Site>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Site>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(newName, site)).Returns(false);
            //Act
            var result = site.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeTrue();
            site.Name.Should().Be(newName);
        }
        [Fact]
        public void Site_Rename_ReturnResultFail()
        {
            //Arrange
            var site = A.Fake<Site>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Site>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(newName, site)).Returns(true);
            //Act
            var result = site.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
    }
}
