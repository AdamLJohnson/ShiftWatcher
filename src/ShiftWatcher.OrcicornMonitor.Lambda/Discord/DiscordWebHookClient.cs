using ShiftWatcher.Models;
using Amazon.EventBridge;

namespace ShiftWatcher.OrcicornMonitor.Lambda.Discord
{
    public interface INewShiftCodeNotifier
    {
        Task SendAsync(CodeInfo codeInfo);
    }
    public class NewShiftCodeNotifier : INewShiftCodeNotifier
    {
        public async Task SendAsync(CodeInfo codeInfo)
        {
            using var client = new AmazonEventBridgeClient();
            var response = await client.PutEventsAsync(new Amazon.EventBridge.Model.PutEventsRequest()
            {
                Entries = new List<Amazon.EventBridge.Model.PutEventsRequestEntry>()
                {
                    new Amazon.EventBridge.Model.PutEventsRequestEntry()
                    {
                        Source = "ShiftWatcher",
                        DetailType = "new-shift-code",
                        EventBusName = "default",
                        Detail = System.Text.Json.JsonSerializer.Serialize(codeInfo)
                    }
                }
            });
        }
    }
}
