namespace Checking_stocks_using_GPT
{
    using Microsoft.Extensions.Options;
    using MongoDB.Driver;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class MongoDBService
    {
        private readonly IMongoCollection<StockPrediction> _predictions;

        public MongoDBService(IMongoDatabase database, IOptions<MongoDBSettings> settings)
        {
            _predictions = database.GetCollection<StockPrediction>(settings.Value.CollectionName);
        }

        public async Task<List<StockPrediction>> GetAllAsync()
        {
            return await _predictions.Find(_ => true).ToListAsync();
        }

        public async Task<StockPrediction> GetByIdAsync(string id)
        {
            return await _predictions.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(StockPrediction prediction)
        {
            await _predictions.InsertOneAsync(prediction);
        }

        // Additional CRUD operations can be added as needed
    }

}
