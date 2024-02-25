using System.Text.Json;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace Roblox.Libraries.EasyJwt;

public class MicrosoftJsonSerialize : IJsonSerializer
{
    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public T Deserialize<T>(string json)
    {
        var result = JsonSerializer.Deserialize<T>(json);
        if (result == null)
            throw new Exception("JsonSerialize.Deserialize returned null");
        return result!;
    }
}

public class EasyJwt
{
    // JWT Config
    private static readonly IJwtAlgorithm Algorithm = new HMACSHA512Algorithm();
    private static readonly IJsonSerializer Serializer = new MicrosoftJsonSerialize();
    private static readonly IBase64UrlEncoder UrlEncoder = new JwtBase64UrlEncoder();
    private static readonly IDateTimeProvider DateTimeProvider = new UtcDateTimeProvider();
    private static readonly IJwtValidator Validator = new JwtValidator(Serializer, DateTimeProvider);
    private static readonly IJwtEncoder Encoder = new JwtEncoder(Algorithm, Serializer, UrlEncoder);
    private static readonly IJwtDecoder Decoder = new JwtDecoder(Serializer, Validator, UrlEncoder, Algorithm);
    
    public string CreateJwt<T>(T obj, string secretKey)
    {
        var token = Encoder.Encode(obj, secretKey);
        if (token == null) throw new NullReferenceException();
        return token;
    }
    
    public T DecodeJwt<T>(string token, string secretKey)
    {
        var json = Decoder.Decode(token, secretKey, true);
        if (json == null) throw new NullReferenceException();
        var result = JsonSerializer.Deserialize<T>(json);
        if (result == null) throw new NullReferenceException();
        return result;
    }
}