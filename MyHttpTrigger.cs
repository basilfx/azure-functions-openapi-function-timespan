using System;
using System.Net;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace Deftpower.Api.Webhooks.Functions.Http.Debugger.Sessions;

public static class MyHttpTrigger
{
    [FunctionName(nameof(MyHttpTrigger))]
    [OpenApiOperation("my-trigger")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MyInstance))]
    public static IActionResult RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "trigger")]
        HttpRequest request)
    {
        return new OkObjectResult(new MyInstance
        {
            MyTimeSpan = TimeSpan.FromHours(1),
            MyNullableTimeSpan = TimeSpan.FromHours(1),
            MyTimeSpanWithConverter = TimeSpan.FromHours(1)
        });
    }
}

public class MyInstance
{
    [JsonProperty("myTimeSpan", Required = Required.Always)]
    public TimeSpan MyTimeSpan { get; set; }

    [JsonProperty("myNullableTimeSpan", Required = Required.AllowNull)]
    public TimeSpan? MyNullableTimeSpan { get; set; }

    [JsonProperty("myTimeSpanWithConverter", Required = Required.Always)]
    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan? MyTimeSpanWithConverter { get; set; }
}

// Copied from https://stackoverflow.com/a/14203972/1423623
public class TimeSpanConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var ts = (TimeSpan) value;
        var tsString = XmlConvert.ToString(ts);
        serializer.Serialize(writer, tsString);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        return XmlConvert.ToTimeSpan(value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof (TimeSpan) || objectType == typeof (TimeSpan?);
    }
}
