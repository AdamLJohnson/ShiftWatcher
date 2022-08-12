using Amazon.DynamoDBv2;

namespace ShiftWatcher.OrcicornMonitor.Lambda.Services;
public class DynamoDbPersistantStorage : PersistantStorage
{
    public DynamoDbPersistantStorage(string tableName) : base(tableName)
    {
    }

    public override async Task DeleteAsync<T>(string key)
    {
        using var client = new AmazonDynamoDBClient();
        var response = await client.DeleteItemAsync(_tableName, new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>()
        {
            { "Code", new Amazon.DynamoDBv2.Model.AttributeValue(key)}
        });
    }

    public override async Task<PersistantStorageGetResult<T>> GetAsync<T>(string key)
    {
        using var client = new AmazonDynamoDBClient();
        var response = await client.GetItemAsync(_tableName, new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>()
        {
            { "Code", new Amazon.DynamoDBv2.Model.AttributeValue(key) }
        });
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || response.Item.Count == 0)
            return new PersistantStorageGetResult<T>(false, key, default);

        var output = System.Text.Json.JsonSerializer.Deserialize<T>(response.Item["Value"].S);
        if (output == null)
            return new PersistantStorageGetResult<T>(false, key, default);

        return new PersistantStorageGetResult<T>(true, key, output);
    }

    public override async Task InsertAsync<T>(string key, T value)
    {
        using var client = new AmazonDynamoDBClient();
        var item = System.Text.Json.JsonSerializer.Serialize(value);
        var response = await client.PutItemAsync(_tableName, new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>()
        {
            { "Code", new Amazon.DynamoDBv2.Model.AttributeValue(key) },
            { "Value", new Amazon.DynamoDBv2.Model.AttributeValue(item)}
        });
    }

    public override Task UpdateAsync<T>(string key, T value)
    {
        return InsertAsync<T>(key, value);
    }
}
