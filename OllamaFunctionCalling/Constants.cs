using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace OllamaFunctionCalling;

public static class Constants
{
    public static OpenAIPromptExecutionSettings ExecutionSettings = new() // You can globally cache this and use it for all calls
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
}