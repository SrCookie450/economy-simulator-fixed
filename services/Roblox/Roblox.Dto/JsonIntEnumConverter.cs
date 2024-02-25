using System.Text.Json;
using System.Text.Json.Serialization;

namespace Roblox;

public class JsonIntEnumConverter<T> : JsonConverter<T> where T : Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        Enum test = (Enum)Enum.Parse(typeof(T), value.ToString());
        writer.WriteNumberValue(Convert.ToInt32(test));
    }
}
