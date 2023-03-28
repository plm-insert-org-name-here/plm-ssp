using Domain.Common;
using Domain.Entities.TaskAggregate;
using FakeItEasy;
using FluentAssertions;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = Domain.Entities.TaskAggregate.Object;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Tests.Entities.TaskAggregate
{
    public class TaskTests
    {
        [Fact]
        public void Task_CreateInstance_ReturnResultOK()
        {
            //Arrange
            var task = A.Fake<Task>();
            //Act
            var result = task.CreateInstance();
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeTrue();
            task.Instances.Should().HaveCount(1);
        }
        [Fact]
        public void Task_CreateInstance_ReturnResultFail()
        {
            //Arrange
            var task = A.Fake<Task>();
            task.OngoingInstance = A.Fake<TaskInstance>();
            //Act
            var result = task.CreateInstance();
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public void Task_AddObjects_ObjectsAdded()
        {
            //Arrange
            var task = A.Fake<Task>();
            Object object1 = new Object("test1");
            Object object2 = new Object("test2");
            List<Object> objects = new List<Object>();
            objects.Add(object1);
            objects.Add(object2);
            //Act
            task.AddObjects(objects);
            //Assert
            task.Objects.Should().HaveCount(2);
        }
        [Fact]
        public void Task_AddSteps_StepsAdded()
        {
            //Arrange
            var task = A.Fake<Task>();
            Step step1 = new Step(0);
            Step step2 = new Step(1);
            List<Step> steps = new List<Step>();
            steps.Add(step1);
            steps.Add(step2);
            //Act
            task.AddSteps(steps);
            //Assert
            task.Steps.Should().HaveCount(2);
        }
        [Fact]
        public void Task_ModifyObject_ReturnResultOK()
        {
            //Arrange
            var task = A.Fake<Task>();
            Object object1 = new Object("test1");
            object1.Id = 1;
            task.Objects.Add(object1);

            int objectId = 1;
            string name = "newName";
            var objectCoordinates = A.Fake<ObjectCoordinates>();
            objectCoordinates.X = 300;
            objectCoordinates.Height = 3;
            objectCoordinates.Y = 300;
            objectCoordinates.Width = 3;
            //Act
            var result = task.ModifyObject(objectId, name, objectCoordinates);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeTrue();
            task.Objects.FirstOrDefault(o => o.Id == objectId).Name.Should().Be(name);
            task.Objects.FirstOrDefault(o => o.Id == objectId).Coordinates.X.Should().Be(objectCoordinates.X);
        }
        [Fact]
        public void Task_ModifyObject_ReturnResultFail_v1_ObjectNull()
        {
            //Arrange
            var task = A.Fake<Task>();
            Object object1 = new Object("test1");
            object1.Id = 1;
            task.Objects.Add(object1);

            int objectId = 0;
            string name = "newName";
            var objectCoordinates = A.Fake<ObjectCoordinates>();
            objectCoordinates.X = 300;
            objectCoordinates.Height = 3;
            objectCoordinates.Y = 300;
            objectCoordinates.Width = 3;
            //Act
            var result = task.ModifyObject(objectId, name, objectCoordinates);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public void Task_ModifyObject_ReturnResultFail_v2_RenameFail()
        {
            //Arrange
            var task = A.Fake<Task>();
            Object object1 = new Object("test1");
            object1.Id = 1;
            Object object2 = new Object("test2");
            object1.Id = 2;
            task.Objects.Add(object1);
            task.Objects.Add(object2);

            int objectId = 1;
            string name = "test2";
            var objectCoordinates = A.Fake<ObjectCoordinates>();
            objectCoordinates.X = 300;
            objectCoordinates.Height = 3;
            objectCoordinates.Y = 300;
            objectCoordinates.Width = 3;
            //Act
            var result = task.ModifyObject(objectId, name, objectCoordinates);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public void Task_ModifyObject_ReturnResultFail_v3_CoordinatesFail()
        {
            //Arrange
            var task = A.Fake<Task>();
            Object object1 = new Object("test1");
            object1.Id = 1;
            task.Objects.Add(object1);

            int objectId = 1;
            string name = "newName";
            var objectCoordinates = A.Fake<ObjectCoordinates>();
            objectCoordinates.X = 10000;
            objectCoordinates.Height = 3;
            objectCoordinates.Y = 300;
            objectCoordinates.Width = 3;
            //Act
            var result = task.ModifyObject(objectId, name, objectCoordinates);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public void Task_ModifyStep_ReturnResultOK()
        {
            //Arrange
            var task = A.Fake<Task>();
            Step step1 = new Step(1);
            task.Steps.Add(step1);

            int stepId = 1;
            int orderNum = 1;
            TemplateState templateStateInit = TemplateState.Present;
            TemplateState templateStateSubs = TemplateState.Present;
            var testObject = A.Fake<Object>();
            //Act
            var result = task.ModifyStep(stepId, orderNum, templateStateInit, templateStateSubs, testObject);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeTrue();
            task.Steps.FirstOrDefault(o => o.Id == stepId).OrderNum.Should().Be(orderNum);
            task.Steps.FirstOrDefault(o => o.Id == stepId).ExpectedInitialState.Should().Be(templateStateInit);
            task.Steps.FirstOrDefault(o => o.Id == stepId).ExpectedSubsequentState.Should().Be(templateStateSubs);
            task.Steps.FirstOrDefault(o => o.Id == stepId).Object.Should().Be(testObject);
        }
        [Fact]
        public void Task_ModifyStep_ReturnResultFail_v1_StepNull()
        {
            //Arrange
            var task = A.Fake<Task>();
            Step step1 = new Step(0);
            task.Steps.Add(step1);

            int stepId = 1;
            int orderNum = 1;
            TemplateState templateStateInit = TemplateState.Present;
            TemplateState templateStateSubs = TemplateState.Present;
            var testObject = A.Fake<Object>();
            //Act
            var result = task.ModifyStep(stepId, orderNum, templateStateInit, templateStateSubs, testObject);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public void Task_ModifyStep_ReturnResultFail_v2_UpdateFail()
        {
            //Arrange
            var task = A.Fake<Task>();
            Step step1 = new Step(1);
            task.Steps.Add(step1);

            int stepId = 1;
            int orderNum = 1;
            TemplateState templateStateInit = TemplateState.UnknownObject;
            TemplateState templateStateSubs = TemplateState.Present;
            var testObject = A.Fake<Object>();
            //Act
            var result = task.ModifyStep(stepId, orderNum, templateStateInit, templateStateSubs, testObject);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
        }
    }
}
