using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Common.DetectorCommand;

namespace Domain.Serializers;

public class DetectorCommandJsonConverter : JsonConverter<DetectorCommand>
{
    private static DetectorCommand.StartDetection? ReadStartDetectionCommand(ref Utf8JsonReader reader)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propName = reader.GetString() ?? "";
                reader.Read();

                if (propName.Equals(
                        SerializationUtils.GetJsonPropertyName<DetectorCommand.StartDetection>(c => c.TaskId)))
                {
                    var taskId = reader.GetInt32();
                    return new DetectorCommand.StartDetection(taskId);
                }
            }
        }

        throw new JsonException("Did not read taskid field");
    }

    public override DetectorCommand? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        DetectorCommand? command = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propName = reader.GetString() ?? "";
                reader.Read();

                if (propName.Equals(SerializationUtils.GetJsonPropertyName<DetectorCommand>(c => c.Message)))
                {
                    var message = reader.GetString();
                    command = message switch
                    {
                        DetectorCommand.RestartMessage => new DetectorCommand.Restart(),
                        DetectorCommand.StartDetectionMessage => ReadStartDetectionCommand(ref reader),
                        DetectorCommand.StopDetectionMessage => new DetectorCommand.StopDetection(),
                        DetectorCommand.ResumeDetectionMessage => new DetectorCommand.ResumeDetection(),
                        DetectorCommand.PauseDetectionMessage => new DetectorCommand.PauseDetection(),
                        _ => command
                    };
                }
            }
        }

        return command;
    }

    public override void Write(Utf8JsonWriter writer, DetectorCommand value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(SerializationUtils.GetJsonPropertyName<DetectorCommand>(c => c.Message), value.Message);

        if (value.GetType() == typeof(DetectorCommand.StartDetection))
        {
            var startCommand = (DetectorCommand.StartDetection)value;
            writer.WriteNumber(
                SerializationUtils.GetJsonPropertyName<DetectorCommand.StartDetection>(c => c.TaskId),
                startCommand.TaskId
            );
        }
        else
        {
            writer.WriteString(
                SerializationUtils.GetJsonPropertyName<DetectorCommand.StartDetection>(c => c.TaskId),
                "dingdong"
            );
        }

        writer.WriteEndObject();
    }
}