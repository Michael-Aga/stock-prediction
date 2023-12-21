namespace Checking_stocks_using_GPT {
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

    public class StockPrediction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string StockSymbol { get; set; }
        public string Prediction { get; set; }
        public DateTime Date { get; set; }
    }
}
