using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using OllamaFunctionCalling.Plugins;

namespace OllamaFunctionCalling;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

public static class OllamaService
{
    public static IServiceCollection AddOllamaService(this IServiceCollection services)
    {
        services.AddKernel().Plugins.AddFromType<SentimentPlugin>();
        services.AddOpenAIChatCompletion("llama3.2:3b", 
            new Uri("http://localhost:11434/v1")); // ADD /v1 to the end of the URL to for the OpenAI Connector.
        
        return services;
    }
}