using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

public class LanguageRulesArrayConverter : JsonConverter<ConcurrentDictionary<string, string[]>>
{
    public override ConcurrentDictionary<string, string[]> Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options)
{
    var dict = new ConcurrentDictionary<string, string[]>();

    if (reader.TokenType != JsonTokenType.StartArray)
        throw new JsonException("Expected start of array for 'rules'");

    while (reader.Read())
    {
        if (reader.TokenType == JsonTokenType.EndArray)
            break;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected inner [key, values] array");

        reader.Read();
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string key");
        string key = reader.GetString()!;

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of values array");

        var values = JsonSerializer.Deserialize<string[]>(ref reader, options)!;
        dict[key] = values;

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of inner [key, values] array");
    }

    return dict;
}


    public override void Write(Utf8JsonWriter writer, ConcurrentDictionary<string, string[]> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var kvp in value)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}
