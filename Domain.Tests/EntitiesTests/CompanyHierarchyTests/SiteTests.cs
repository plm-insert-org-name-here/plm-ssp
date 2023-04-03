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
        public void Site_AddChildNode_ReturnResultOK()
        {
            //Arrange
            var site = A.Fake<Site>();
            string opuName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Site ,OPU>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(site, opuName, null)).Returns(false);
            //Act
            var result = site.AddChildNode(opuName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<OPU>>();
            result.IsSuccess.Should().BeTrue();
            site.Children.Count.Should().Be(1);
        }
        [Fact]
        public void Site_AddChildNode_ReturnResultFail()
        {
            //Arrange
            var site = A.Fake<Site>();
            string opuName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Site, OPU>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(site, opuName, null)).Returns(true);
            //Act
            var result = site.AddChildNode(opuName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<OPU>>();
            result.IsSuccess.Should().BeFalse();
        }
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
        [Fact]
        public void Site_New_ReturnResultOK()
        {
            //Arrange
            var site = A.Fake<Site>();
            site.Name = "test";
            string name = "test";
            var nameUniquenessChechker = A.Fake<ICHNameUniquenessChecker<Site>>();
            A.CallTo(() => nameUniquenessChechker.IsDuplicate(name,null)).Returns(false);
            //Act
            var result = Site.New(name, nameUniquenessChechker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<Site>>();
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(site.Name);
        }
        [Fact]
        public void Site_New_ReturnResultFail()
        {
            //Arrange
            string name = "test";
            var nameUniquenessChechker = A.Fake<ICHNameUniquenessChecker<Site>>();
            A.CallTo(() => nameUniquenessChechker.IsDuplicate(name, null)).Returns(true);
            //Act
            var result = Site.New(name, nameUniquenessChechker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<Site>>();
            result.IsSuccess.Should().BeFalse();
        }
    }
}
