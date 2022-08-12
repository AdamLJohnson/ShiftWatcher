using Amazon.Lambda.Core;
using ShiftWatcher.OrcicornMonitor.Lambda.Discord;
using ShiftWatcher.OrcicornMonitor.Lambda.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ShiftWatcher.OrcicornMonitor.Lambda;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(ILambdaContext context)
    {
        var tableName = System.Environment.GetEnvironmentVariable("code_dynamo_table") ?? "";
        var webClient = new HttpRequestFactory().Create();
        var persistantStorage = new DynamoDbPersistantStorage(tableName);
        var newShiftCodeNotifier = new NewShiftCodeNotifier();
        var result = await new OrcicornClient(webClient, persistantStorage, newShiftCodeNotifier).ProcessAsync("https://shift.orcicorn.com/tags/wonderlands/index.json");
    }
}
