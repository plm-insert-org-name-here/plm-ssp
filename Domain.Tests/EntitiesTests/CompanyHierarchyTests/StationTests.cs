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
    public class StationTests
    {
        [Fact]
        public void Station_AddChildNode_ReturnResultOK()
        {
            //Arrange
            var station = A.Fake<Station>();
            string locationName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Station, Location>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(station, locationName, null)).Returns(false);
            //Act
            var result = station.AddChildNode(locationName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<Location>>();
            result.IsSuccess.Should().BeTrue();
            station.Children.Count.Should().Be(1);
        }
        [Fact]
        public void Station_AddChildNode_ReturnResultFail()
        {
            //Arrange
            var station = A.Fake<Station>();
            string locationName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Station, Location>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(station, locationName, null)).Returns(true);
            //Act
            var result = station.AddChildNode(locationName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<Location>>();
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public void Station_Rename_ReturnResultOK()
        {
            //Arrange
            var station = A.Fake<Station>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Line, Station>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(station.Parent,newName, station)).Returns(false);
            //Act
            var result = station.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeTrue();
            station.Name.Should().Be(newName);
        }
        [Fact]
        public void Station_Rename_ReturnResultFail()
        {
            //Arrange
            var station = A.Fake<Station>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Line, Station>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(station.Parent, newName, station)).Returns(true);
            //Act
            var result = station.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
    }
}
