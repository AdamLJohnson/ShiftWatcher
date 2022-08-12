using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using ShiftWatcher.Cloud;

var app = new App();

new ShiftWatcherCdkStack(app, "ShiftWatcher", new StackProps
{
    Env = new Amazon.CDK.Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_AWS_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_AWS_REGION")
    }
});

app.Synth();