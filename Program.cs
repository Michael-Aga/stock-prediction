using Checking_stocks_using_GPT;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

/*How to run the program just click the green button on the top with IIS Express
it boots up the SwaggerUI and you get the Gpt3 and Stocks
Gpt3 - press "try it out" and in the symbol write the stock you want to check (4 letters tops)
it will send the stock symbol to the AlphaVantage API and get the stock data
then it will send it to the Gpt3 api with a promt to try and make an educated guees what will happen to the stock in the next week
and keep the answer in the MongoDB Cloud
Stocks - This just gives to the stock data right away from the AlphaVantage API without sending it to Gpt3
and without saving it in the MongoDB Cloud
*/

var builder = WebApplication.CreateBuilder(args);

// Add MongoDB Settings
var mongoDBSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(mongoDBSettings.ConnectionString));

var alphaVantageApiKey = builder.Configuration["AlphaVantageApiKey"];
var gpt3ApiKey = builder.Configuration["Gpt3ApiKey"];

builder.Services.AddHttpClient("AlphaVantageService", client =>
{
    client.BaseAddress = new Uri("https://www.alphavantage.co/");
});

builder.Services.AddTransient(sp => new AlphaVantageService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("AlphaVantageService"), alphaVantageApiKey));

builder.Services.AddHttpClient("Gpt3Client", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {gpt3ApiKey}");
});

builder.Services.AddTransient<Gpt3Service>();

// MongoDB service registration
builder.Services.AddSingleton<IMongoDatabase>(s =>
{
    var client = s.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDBSettings.DatabaseName);
});

builder.Services.AddSingleton<MongoDBService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
