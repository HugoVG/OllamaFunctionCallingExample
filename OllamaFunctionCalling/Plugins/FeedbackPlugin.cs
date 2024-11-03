using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace OllamaFunctionCalling.Plugins;

public class FeedbackPlugin(ILogger<FeedbackPlugin> logger)
{
    [KernelFunction]
    [Description("For summarizing feedback")]
    public void PostSummarizationFeedbackCallback(List<string> Positives = null, List<string> Negatives = null, List<string> Neutrals = null, string guessedProduct = "Unknown")
    {
        StringBuilder stringBuilder = new();
        Positives ??= [];
        Negatives ??= [];
        Neutrals ??= [];
        
        stringBuilder.AppendLine($"Feedback for {guessedProduct}");
        stringBuilder.AppendLine("Positives:");
        foreach (var positive in Positives)
        {
            stringBuilder.AppendLine(positive);
        }
        stringBuilder.AppendLine("Negatives:");
        foreach (var negative in Negatives)
        {
            stringBuilder.AppendLine(negative);
        }
        stringBuilder.AppendLine("Neutrals:");
        foreach (var neutral in Neutrals)
        {
            stringBuilder.AppendLine(neutral);
        }
        logger.LogInformation(stringBuilder?.ToString());
    }
}