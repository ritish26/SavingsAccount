using Domain.Views;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Projections;

public interface IMongoContext
{
    IMongoCollection<T> GetCollection<T>();
}

public class MongoContext : IMongoContext
{
    private readonly IConfiguration _configuration;
    
    private MongoClient MongoClient { get; set;  }
     
    private IMongoDatabase Database { get; set; }

    private Dictionary<Type, string> CollectionTypeNameMap =>
        new()
        {
            { typeof(SavingsAccountView), "Savingsaccounts" },
            { typeof(TenantProjectionCheckpoint), "tenantcheckpoints" },
        };

    public MongoContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public IMongoCollection<T> GetCollection<T>()
    {
        ConfigureMongo();

        if (!CollectionTypeNameMap.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException($"The type {typeof(T)} does not have a mapped collection.");
        }
        
        return Database.GetCollection<T>(CollectionTypeNameMap[typeof(T)]);
    }

    private void ConfigureMongo()
    {
        //Exit if mongo client is already initalized
        if (MongoClient != null)
        {
            return;
        }
        
        // Register BSON class maps for serialization
        RegisterClassMap();
        var settings = MongoClientSettings.FromConnectionString(
            _configuration.GetSection("MongoDbSettings:ConnectionString").Value);
        
        //Configure mongo (you can inject the config just to simplify
        MongoClient = new MongoClient(settings);
        Database = MongoClient.GetDatabase(_configuration.GetSection("MongoDbSettings:Database").Value);
    }

    private void RegisterClassMap()
    {
        RegisterClassMap<SavingsAccountView>(classMap =>
        {
            classMap.AutoMap();
            classMap.MapIdField(x => x.Id); 
        });

        RegisterClassMap<TenantProjectionCheckpoint>(classMap =>
        {
            classMap.AutoMap();
            classMap.MapIdField(x => x.TenantId);
        });
    }

    private void RegisterClassMap<T>(Action<BsonClassMap<T>> map)
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
        {
            BsonClassMap.RegisterClassMap<T>(map);
        }
    }
}