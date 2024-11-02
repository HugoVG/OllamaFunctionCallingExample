using System.Diagnostics;
using Bogus.DataSets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OllamaFunctionCalling;
using OllamaFunctionCalling.Plugins;
using OllamaFunctionCalling.Services;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

var startStamp = Stopwatch.GetTimestamp();

// Create a generic host to show how it would look like in a real application

var builder = Host.CreateApplicationBuilder(args);

// Add the Ollama service to the host
builder.Services.AddOllamaService();
builder.Services.AddSingleton<IDatabase,MockDatabase>();

var host = builder.Build();

// You get these in any service, I just wanted to show this in fewer files.
var chatCompletionService = host.Services.GetRequiredService<IChatCompletionService>();
var kernel = host.Services.GetRequiredService<Kernel>(); // <-- you need this to call the functions

var mockDb = host.Services.GetRequiredService<IDatabase>();

OpenAIPromptExecutionSettings openAiPromptExecutionSettings =
    new() // You can globally cache this and use it for all calls
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

var logger = host.Services.GetRequiredService<ILogger<Program>>();


logger.LogInformation(
    "Kernel creation took: {Time}", TimeSpan.FromTicks(Stopwatch.GetTimestamp()) - TimeSpan.FromTicks(startStamp));


int amount = 10;
foreach (var statement in mockDb.GetStatementsToProcess().Take(amount).ToList())
{
    var userPrompt = $"id: {statement.Id}; statement: {statement.Statement}";
    var history = new ChatHistory(); // Make new chat history for each message

    const string systemPrompt =
        """
        you're an AI assistant. 
        Your task is to tell if the sentiment in a message is positive, negative or neutral and to tell the polarity of the sentiment. 
        After you have decided the Sentiment and Polarity call the PostSentimentFunction with the message and the sentiment. 
        do not forget to call them. Keep in mind to respond as short as possible.
        """;
    
    history.AddSystemMessage(systemPrompt);

    history.AddUserMessage(userPrompt);
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAiPromptExecutionSettings,
        kernel: kernel);
    logger.LogInformation("Assistant 1 > {Result}", result.Content);
}

logger.LogInformation(
    "Generating {Amount} took: {Time}", amount,
    TimeSpan.FromTicks(Stopwatch.GetTimestamp()) - TimeSpan.FromTicks(startStamp));
logger.LogInformation(
    "Successful calls: {Total} Failed calls: {Failed}", mockDb.GetSentiments().Count, amount - mockDb.GetSentiments().Count);