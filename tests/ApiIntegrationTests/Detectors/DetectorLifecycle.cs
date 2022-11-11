using System.Net;
using System.Text;
using System.Text.Json;
using Api;
using Domain.Common;
using Domain.Common.DetectorCommand;
using FastEndpoints;
using Xunit;
using Xunit.Abstractions;
using Xunit.Priority;

namespace ApiIntegrationTests.Detectors;

using IdentifyEP = Api.Endpoints.Detectors.Identify;
using CommandEP = Api.Endpoints.Detectors.Command;
using EventEP = Api.Endpoints.Events.Create;
using TaskEP = Api.Endpoints.Tasks.GetById;

[Collection("Sequential")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class DetectorLifecycle : IClassFixture<Setup>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public DetectorLifecycle(Setup setup, ITestOutputHelper output)
    {
        _output = output;
        _client = setup.Client;
    }

    private async Task CanSendSuccessEvent(EventEP.Req req)
    {
        var (response, _) = await _client.POSTAsync<EventEP, EventEP.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // TODO(rg): more descriptive name
    [Fact, Priority(0)]
    public async Task DetectorCanIdentifyItselfAndPerformSimpleTask()
    {
        IdentifyEP.Req identifyReq = new()
        {
            LocationId = 2,
            MacAddress = "12:34:56:78:90:AB",
            Coordinates = new List<IdentifyEP.Req.CalibrationCoordsReq>()
        };

        var (identityResponse, _) = await _client.POSTAsync<IdentifyEP, IdentifyEP.Req, EmptyResponse>(identifyReq);

        Assert.NotNull(identityResponse);
        Assert.Equal(HttpStatusCode.NoContent, identityResponse.StatusCode);

        // TODO: check device status (standby)

        CommandEP.Req commandReq = new()
        {
            Id = 2,
            Command = new DetectorCommand.StartDetection(2)
        };

        var (commandResponse, _) = await _client.POSTAsync<CommandEP, CommandEP.Req, EmptyResponse>(commandReq);

        Assert.NotNull(commandResponse);
        Assert.Equal(HttpStatusCode.NoContent, commandResponse.StatusCode);

        // TODO: check device status (monitoring)

        await CanSendSuccessEvent(
            new EventEP.Req
            {
                TaskId = 2,
                StepId = 4,
                Success = true
            });

        await CanSendSuccessEvent(
            new EventEP.Req
            {
                TaskId = 2,
                StepId = 5,
                Success = true
            });

        await CanSendSuccessEvent(
            new EventEP.Req
            {
                TaskId = 2,
                StepId = 6,
                Success = true
            });

        var (taskResponse, taskResult) = await _client.GETAsync<TaskEP, TaskEP.Req, TaskEP.Res>(new TaskEP.Req { Id = 2 });

        Assert.NotNull(taskResponse);
        Assert.Equal(HttpStatusCode.OK, taskResponse.StatusCode);
        Assert.NotNull(taskResult);
        Assert.Equal(TaskState.Inactive, taskResult.State);

        // TODO: check device status(standby)
        // TODO: check task instance status & events (completed & 3)
    }
}