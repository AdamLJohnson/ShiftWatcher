using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;
using CSharpDiscordWebhook.NET.Discord;
using ShiftWatcher.Models;
using System.Drawing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ShiftWatcher.DiscordSender.Lambda;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(CloudWatchEvent<CodeInfo> eventInfo, ILambdaContext context)
    {
        var codeInfo = eventInfo.Detail;

        DiscordWebhook hook = new DiscordWebhook();        
        hook.Uri = new Uri(System.Environment.GetEnvironmentVariable("discord_webhook_url"));
        DiscordMessage message = new DiscordMessage();
        message.Content = "New SHIFT Code";
        message.Username = "SHIFT Code Bot";

        DiscordEmbed embed = new DiscordEmbed();
        embed.Title = "SHIFT Code";
        //embed.Description = "Embed description";
        embed.Url = codeInfo.Link;
        embed.Timestamp = DateTime.Now;
        embed.Color = Color.Orange;

        embed.Fields = new List<EmbedField>();
        embed.Fields.Add(new EmbedField() { Name = "Code", Value = codeInfo.Code, InLine = true });
        embed.Fields.Add(new EmbedField() { Name = "Reward", Value = codeInfo.Reward, InLine = true });
        embed.Fields.Add(new EmbedField() { Name = "Platform", Value = codeInfo.Platform, InLine = true });
        embed.Fields.Add(new EmbedField() { Name = "Issued", Value = codeInfo.Archived, InLine = true });
        embed.Fields.Add(new EmbedField() { Name = "Expires", Value = codeInfo.Expires, InLine = true });

        message.Embeds = new List<DiscordEmbed>();
        message.Embeds.Add(embed);

        await hook.SendAsync(message);
    }
}
