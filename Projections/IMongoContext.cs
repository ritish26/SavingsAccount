using MongoDB.Driver;

namespace Projections;

public interface IMongoContext
{
    IMongoCollection<T> GetCollection<T>();
}