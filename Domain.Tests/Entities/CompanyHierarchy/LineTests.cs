using Application.Services;
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
    public class LineTests
    {
        [Fact]
        public void Line_AddChildNode_ReturnResultOK()
        {
            //Arrange
            var line = A.Fake<Line>();
            string stationName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Line,Station>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(line,stationName,null)).Returns(false);
            //Act
            var result = line.AddChildNode(stationName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<Station>>();
            result.IsSuccess.Should().BeTrue();
            line.Children.Count.Should().Be(1);
        }
        [Fact]
        public void Line_AddChildNode_ReturnResultFail()
        {
            //Arrange
            var line = A.Fake<Line>();
            string stationName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<Line, Station>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(line, stationName, null)).Returns(true);
            //Act
            var result = line.AddChildNode(stationName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result<Station>>();
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public void Line_Rename_ReturnResultOK()
        {
            //Arrange
            var line = A.Fake<Line>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<OPU,Line>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(line.Parent, newName, line)).Returns(false);
            //Act
            var result = line.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeTrue();
            line.Name.Should().Be(newName);
        }
        [Fact]
        public void Line_Rename_ReturnResultFail()
        {
            //Arrange
            var line = A.Fake<Line>();
            string newName = "test";
            var nameUniquenessChecker = A.Fake<ICHNameUniquenessChecker<OPU, Line>>();
            A.CallTo(() => nameUniquenessChecker.IsDuplicate(line.Parent, newName, line)).Returns(true);
            //Act
            var result = line.Rename(newName, nameUniquenessChecker);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
    }
}
