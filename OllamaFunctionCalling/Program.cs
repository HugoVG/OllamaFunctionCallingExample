using System.Diagnostics;
using Bogus.DataSets;
using ExampleData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.VisualBasic.FileIO;
using OllamaFunctionCalling;
using OllamaFunctionCalling.Plugins;
using OllamaFunctionCalling.Services;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

var startStamp = Stopwatch.GetTimestamp();

// Create a generic host to show how it would look like in a real application

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders()
    .AddFilter("Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService", LogLevel.Error)
    .AddConsole();
// Add the Ollama service to the host
builder.Services.AddOllamaService();
builder.Services.AddSingleton<IDatabase, MockDatabase>();
builder.Services.AddScoped<SentimentAnalysisService>();

var host = builder.Build();

// You get these in any service, I just wanted to show this in fewer files.
var chatCompletionService = host.Services.GetRequiredService<IChatCompletionService>();
var kernel = host.Services.GetRequiredService<Kernel>(); // <-- you need this to call the functions

var mockDb = host.Services.GetRequiredService<IDatabase>();
var analyser = host.Services.GetRequiredService<SentimentAnalysisService>();

OpenAIPromptExecutionSettings openAiPromptExecutionSettings =
    new() // You can globally cache this and use it for all calls
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

var logger = host.Services.GetRequiredService<ILogger<Program>>();


logger.LogInformation(
    "Kernel creation took: {Time}", TimeSpan.FromTicks(Stopwatch.GetTimestamp()) - TimeSpan.FromTicks(startStamp));


// int amount = 1000000;
//
// var summary = await analyser.Summarize(mockDb.GetStatementsToProcess().Take(amount / 100).Select(x=>x.Statement).ToList(), "These are random reviews for specific products from out website, if it doesn't look like a review or about a products, please ignore it. give back any feedback we can use to improve our product preferably in dutch.");
// logger.LogInformation(
//     "Summarizing took: {Time}", TimeSpan.FromTicks(Stopwatch.GetTimestamp()) - TimeSpan.FromTicks(startStamp));
// logger.LogInformation("Summary: {Summary}", summary);
//
// var summary2 = await analyser.Summarize(mockDb.GetStatementsToProcess("chewing gum").Take(amount).Select(x=>x.Statement).ToList(), "These are reviews for chewing gum, if it isn't a review or about chewing gum, please ignore it. give back any feedback we can use to improve our product preferably in dutch.");
// logger.LogInformation(
//     "Summarizing took: {Time}", TimeSpan.FromTicks(Stopwatch.GetTimestamp()) - TimeSpan.FromTicks(startStamp));
// logger.LogInformation("Summary: {Summary}", summary2);

;
var reviews = FileReader.ReadFile("{Environment.SpecialFolder.CommonStartup}\\Downloads\\Electronics.jsonl");

var reviewForProduct = reviews
    .Take(500000) // Take 500k reviews not the 32gb worth of reviews
    .GroupBy(x => x.ProductId) // Group by product
    .Where(x => x.Count() > 5) // We only want products with more than 5 reviews
    .ToList();

Dictionary<string, string?> productSummaries = new();

logger.LogInformation("Querying {Amount} products", reviewForProduct.Count);

foreach (var productReviews in reviewForProduct)
{
    var productStartStamp = Stopwatch.GetTimestamp();
    var productSummary = await analyser.Summarize(productReviews.Select(x => x.ReviewText).ToList(),
        "These are random reviews for specific products from our website, give back any feedback we can use to improve our product via the PostSummarizationFeedbackCallback tool.");
    logger.LogInformation(
        "Summarizing took: {Time}",
        TimeSpan.FromTicks(Stopwatch.GetTimestamp()) - TimeSpan.FromTicks(productStartStamp));
    logger.LogInformation("Summary: {Summary}", productSummary);
    productSummaries.Add(productReviews.Key, productSummary);
}


foreach (var keyValuePair in productSummaries)
{
    logger.LogInformation("Product: {Product} Summary: {Summary}", keyValuePair.Key, keyValuePair.Value);
}