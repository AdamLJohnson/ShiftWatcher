using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftWatcher.Cloud
{
    internal class ShiftWatcherCdkStack : Stack
    {
        internal ShiftWatcherCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var webhookUrl = System.Environment.GetEnvironmentVariable("discord_webhook_url") ?? "";
            if (string.IsNullOrEmpty(webhookUrl))
                throw new Exception("discord_webhook_url environment variable is missing");

            IEnumerable<string?> commands = new[]
            {
                "ls",
                "cd /asset-input/src/ShiftWatcher.OrcicornMonitor.Lambda",
                "export DOTNET_CLI_HOME=\"/tmp/DOTNET_CLI_HOME\"",
                "export PATH=\"$PATH:/tmp/DOTNET_CLI_HOME/.dotnet/tools\"",
                "dotnet tool install -g Amazon.Lambda.Tools",
                "dotnet lambda package -o output.zip --msbuild-parameters \"/p:PublishReadyToRun=true --self-contained=false\"",
                "unzip -o -d /asset-output output.zip"
            };

            var codeTable = new Table(this, "shiftWatchCodeTable", new TableProps() 
            { 
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute() { Name = "Code", Type = AttributeType.STRING },
                RemovalPolicy = RemovalPolicy.RETAIN,
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            Function monitorFunction = new Function(this,
                "orcicorn-monitor-function",
                new FunctionProps
                {
                    Runtime = Runtime.DOTNET_6,
                    Timeout = Duration.Seconds(10),
                    Code = Code.FromAsset("../../", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                    {
                        AssetHashType = AssetHashType.OUTPUT,
                        Bundling = new BundlingOptions
                        {
                            Image = Runtime.DOTNET_6.BundlingImage,
                            Command = new[]
                            {
                                "bash", "-c", string.Join(" && ", commands)
                            }
                        }
                    }),
                    Handler = "ShiftWatcher.OrcicornMonitor.Lambda::ShiftWatcher.OrcicornMonitor.Lambda.Function::FunctionHandler"
                }
            );

            var defaultEventBus = EventBus.FromEventBusArn(this, "default_event_bus", $"arn:aws:events:{props.Env.Region}:{props.Env.Account}:event-bus/default");
            defaultEventBus.GrantPutEventsTo(monitorFunction);

            codeTable.GrantReadWriteData(monitorFunction);
            monitorFunction.AddEnvironment("code_dynamo_table", codeTable.TableName);

            var watcherEventRule = new Rule(this, "shiftWatcherRule", new RuleProps() 
            { 
                Schedule = Schedule.Rate(Duration.Minutes(30))
            });

            watcherEventRule.AddTarget(new Amazon.CDK.AWS.Events.Targets.LambdaFunction(monitorFunction));

            commands = new[]
            {
                "ls",
                "cd /asset-input/src/ShiftWatcher.DiscordSender.Lambda",
                "export DOTNET_CLI_HOME=\"/tmp/DOTNET_CLI_HOME\"",
                "export PATH=\"$PATH:/tmp/DOTNET_CLI_HOME/.dotnet/tools\"",
                "dotnet tool install -g Amazon.Lambda.Tools",
                "dotnet lambda package -o output.zip --msbuild-parameters \"/p:PublishReadyToRun=true --self-contained=false\"",
                "unzip -o -d /asset-output output.zip"
            };

            Function discordSenderFunction = new Function(this,
                "discord-sender-function",
                new FunctionProps
                {
                    Runtime = Runtime.DOTNET_6,
                    Timeout = Duration.Seconds(10),
                    Code = Code.FromAsset("../../", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                    {
                        AssetHashType = AssetHashType.OUTPUT,
                        Bundling = new BundlingOptions
                        {
                            Image = Runtime.DOTNET_6.BundlingImage,
                            Command = new[]
                            {
                                "bash", "-c", string.Join(" && ", commands)
                            }
                        }
                    }),
                    Handler = "ShiftWatcher.DiscordSender.Lambda::ShiftWatcher.DiscordSender.Lambda.Function::FunctionHandler",
                    Environment = new Dictionary<string, string>()
                    {
                        {"discord_webhook_url", webhookUrl}
                    }
                }
            );

            var newCodeEventRule = new Rule(this, "shiftWatcherNewCode", new RuleProps()
            {
                EventPattern = new EventPattern() 
                {  
                    Source = new[] { "ShiftWatcher" },
                    DetailType = new[] { "new-shift-code" }
                }
            });

            newCodeEventRule.AddTarget(new Amazon.CDK.AWS.Events.Targets.LambdaFunction(discordSenderFunction));
        }
    }
}
