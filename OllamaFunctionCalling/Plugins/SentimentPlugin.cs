using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OllamaFunctionCalling.Services;

namespace OllamaFunctionCalling.Plugins;


public class SentimentPlugin(ILogger<SentimentPlugin> logger, IDatabase mockDatabase)
{
    
    public static readonly List<ProcessedSentiment> PostSentimentStatements = [];
    
    [KernelFunction]
    [Description("Post after deciding the sentiment")]
    public void PostSentimentFunction(string id,  string message, string sentiment)
    {
        logger.LogInformation("[Sentiment {Id}: {Sentiment}]: ``{Message}``", id, sentiment,message);

        if (!Guid.TryParse(id, out var guid))
        {
            logger.LogError("Invalid Guid format {Id} {Message} {Sentiment}", id, message, sentiment);
            return;
        }
        
        if(sentiment != "positive" && sentiment != "negative" && sentiment != "neutral")
        {
            logger.LogError("Invalid Sentiment {Id} {Message} {Sentiment}", id, message, sentiment);
            return;
        }

        mockDatabase.AddSentiment(new ProcessedSentiment()
        {
            Id = guid,
            Statement = message,
            Sentiment = sentiment,
        });
    }
}