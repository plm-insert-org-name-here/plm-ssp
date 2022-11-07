using System.Text.Json;
using Api.Endpoints.Detectors;
using Domain.Common.DetectorCommand;
using FastEndpoints;
using Stream = System.IO.Stream;

namespace Api.RequestBinders;

public class CommandReqBinder : IRequestBinder<Command.Req>
{
    private async Task<byte[]> ReadBodyToBytes(Stream body)
    {
        using var memoryStream = new MemoryStream();
        
        await body.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private DetectorCommand ReadCommandInner(ref Utf8JsonReader reader)
    {
        var message = (reader.GetString() ?? "").ToLower();
        
        switch (message)
        {
            case DetectorCommand.RestartMessage:
                return new DetectorCommand.Restart();
            case DetectorCommand.StartDetectionMessage:
                int? taskId = null;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var propName = (reader.GetString() ?? "").ToLower();
                        reader.Read();

                        if (propName.Equals(nameof(DetectorCommand.StartDetection.TaskId).ToLower()))
                            taskId = reader.GetInt32();
                    }
                }

                if (!taskId.HasValue)
                    throw new ValidationFailureException("Missing parameter 'taskId'");
                
                return new DetectorCommand.StartDetection(taskId.Value);
            case DetectorCommand.StopDetectionMessage:
                return new DetectorCommand.StopDetection();
            case DetectorCommand.ResumeDetectionMessage:
                return new DetectorCommand.ResumeDetection();
            case DetectorCommand.PauseDetectionMessage:
                return new DetectorCommand.PauseDetection();
            default:
                throw new ValidationFailureException("Invalid command message");
        }
    }
    
    private DetectorCommand? ReadCommand(ref BinderContext ctx)
    {
        var bytes = ReadBodyToBytes(ctx.HttpContext.Request.Body).GetAwaiter().GetResult();
        var reader = new Utf8JsonReader(bytes);

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new ValidationFailureException("Failed to parse JSON contents");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propName = (reader.GetString() ?? "").ToLower();
                reader.Read();

                if (propName.Equals(nameof(DetectorCommand.Message).ToLower()))
                    return ReadCommandInner(ref reader);
            }
        }

        return null;
    }

    private int? ReadId(ref BinderContext ctx)
    {
        var idObj = ctx.HttpContext.Request.RouteValues["id"];
        if (idObj is null) return null;
        var success =  int.TryParse(idObj.ToString(), out var id);
        return success ? id : null;
    }
    
    
    public ValueTask<Command.Req> BindAsync(BinderContext ctx, CancellationToken ct)
    {
        var req = new Command.Req();

        var id = ReadId(ref ctx);
        if (!id.HasValue) return new ValueTask<Command.Req>();

        var command = ReadCommand(ref ctx);
        if (command is null) return new ValueTask<Command.Req>();

        req.Id = id.Value;
        req.Command = command;
        return new ValueTask<Command.Req>(req);
    }
}