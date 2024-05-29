using System;
using System.Runtime;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Messaging;
using FunctionCalling;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.VisualBasic;
using SkUtil.Configs;
using SkUtil.CustomLLM;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("InternalAzureOpenAI");
//建议使用 OpenAI 的最新模型（如 gpt-3.5-turbo-1106 或 gpt-4-1106-preview）以获得最佳的工具调用体验。OpenAI 的最新模型通常具有更好的性能和更高的准确性，因此使用这些模型可以提高工具调用的效果。

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(config.ModelId, endpoint: config.Endpoint, apiKey: config.ApiKey)
    .Build();
kernel.ImportPluginFromFunctions("WeatherPlugin", new[]
{
        kernel.CreateFunctionFromMethod(GetWeatherForCity, "GetWeatherForCity", "获取指定城市的天气")
    });
var template = "我想知道现在北京的天气状况？";

Console.WriteLine("=====>手动function calling");
{
    OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
    {
        Temperature = 0,
        ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
    };
    var chatHistory = new ChatHistory();
    chatHistory.AddSystemMessage("You are a useful assistant.");
    chatHistory.AddUserMessage(template);
    Console.WriteLine($"User: {template}");
    var chat = kernel.GetRequiredService<IChatCompletionService>();
    while (true)
    {
        ChatMessageContent result = await chat.GetChatMessageContentAsync(chatHistory, settings, kernel);
        if (result.Content is not null)
        {
            Console.Write("Assistant:" + result.Content);
        }

        IEnumerable<FunctionCallContent> functionCalls = FunctionCallContent.GetFunctionCalls(result);
        if (!functionCalls.Any())
        {
            break;
        }

        chatHistory.Add(result); // Adding LLM response containing function calls(requests) to chat history as it's required by LLMs.

        foreach (var functionCall in functionCalls)
        {
            try
            {
                FunctionResultContent resultContent = await functionCall.InvokeAsync(kernel); // Executing each function.

                chatHistory.Add(resultContent.ToChatMessage());
            }
            catch (Exception ex)
            {
                chatHistory.Add(new FunctionResultContent(functionCall, ex).ToChatMessage());
                // Adding function result to chat history.
                // Adding exception to chat history.
                // or
                //string message = "Error details that LLM can reason about.";
                //chatHistory.Add(new FunctionResultContent(functionCall, message).ToChatMessageContent()); // Adding function result to chat history.
            }
        }

        Console.WriteLine();
    }
}




Console.WriteLine("=====>自动function calling");
{



    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
    {
        Temperature = 0,
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };
    var chatHistory = new ChatHistory();
    chatHistory.AddSystemMessage("You are a useful assistant.");
    chatHistory.AddUserMessage(template);
    Console.WriteLine($"User: {template}");
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    var result = await chatService.GetChatMessageContentAsync(chatHistory, openAIPromptExecutionSettings, kernel);
    Console.WriteLine("Assistant:" + result.ToString());

}



Console.ReadKey();
static string GetWeatherForCity(string cityName)
{
    return $"{cityName} 25°,天气晴朗。";
}