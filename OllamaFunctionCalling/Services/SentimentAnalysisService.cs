using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace OllamaFunctionCalling.Services;

public class SentimentAnalysisService(
    IChatCompletionService chatCompletionService,
    ILogger<SentimentAnalysisService> logger,
    Kernel kernel)
{
    public async Task AnalyseSentiment(DbStatement dbStatement)
    {
        var userPrompt = $"id: {dbStatement.Id}; statement: {dbStatement.Statement}";
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
            executionSettings: Constants.ExecutionSettings,
            kernel: kernel);
        logger.LogTrace("[Reasoning] > {Result}", result.Content);
    }
    
    public async Task<string?> Summarize(List<string> statements, string context = "")
    {
        var history = new ChatHistory(); // Make new chat history for each message

        string systemPrompt =
            $"""
            you're an AI assistant. 
            Your task is to summarize the feedback. 
            You got task is to summarize the feedback into 3 buckets: Positives, Negatives, and Neutrals.
            After you have decided the feedback, call the PostSummarizationFeedbackCallback with the feedback from those lists.
            Additional context: {context}
            """;
        
        history.AddSystemMessage(systemPrompt);

        var text = string.Join("\n\r", statements);

        history.AddUserMessage(text);
        Stopwatch sw = Stopwatch.StartNew();
        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: Constants.ExecutionSettings, // no tools
            kernel: kernel);
        logger.LogTrace("[Summarize] took {Time} > {Result}", sw.Elapsed, result.Content);
        return result.Content;
    }
}