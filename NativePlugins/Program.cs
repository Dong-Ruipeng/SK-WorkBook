// See https://aka.ms/new-console-template for more information
using SkUtil.Configs;
using NativePlugins;

using SkUtil.CustomLLM;
using Microsoft.SemanticKernel;
using NativePlugins.Plugins.Weather;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
var config = ConfigExtensions.FromConfig<OpenAIConfig>("OneApiSpark");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);

//定义kernel 对象
var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: config.ModelId,
apiKey: config.ApiKey,
httpClient: client).Build();

kernel.ImportPluginFromType<WeatherPlugin>();


OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
{
    Temperature = 0.3,
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};
var chatHistory = new ChatHistory();
chatHistory.AddSystemMessage("You are a useful assistant.");
var input = Console.ReadLine();
chatHistory.AddUserMessage(input);
Console.WriteLine($"User: {input}");
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var result = await chatService.GetChatMessageContentAsync(chatHistory, openAIPromptExecutionSettings, kernel);
Console.WriteLine("Assistant:" + result.ToString());

Console.ReadKey();