using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

using SKPlugins.Core.HttpPlugin;

using SkUtil.Configs;
using SkUtil.CustomLLM;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("InternalAzureOpenAI");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);

var kernel = Kernel.CreateBuilder().
   AddOpenAIChatCompletion(modelId: config.ModelId, apiKey: config.ApiKey, httpClient: client)
    .Build();

var httpPlugin = kernel.ImportPluginFromType<HttpPlugin>();


//var httpclient = new HttpClient();
//kernel.ImportPluginFromObject(new HttpPlugin(httpclient));



var history = new ChatHistory();

// Get chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Start the conversation
Console.Write("User > ");
string? userInput;
while ((userInput = Console.ReadLine()) is not null)
{
    // Add user input
    history.AddUserMessage(userInput);

    // Enable auto function calling
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);

    // Get user input again
    Console.Write("User > ");
}


Console.ReadKey();