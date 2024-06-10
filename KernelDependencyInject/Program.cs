// See https://aka.ms/new-console-template for more information
using SkUtil.Configs;
using KernelDependencyInject;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using KernelDependencyInject.Plugins;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SkUtil.CustomLLM;


var config = ConfigExtensions.FromConfig<OpenAIConfig>("InternalAzureOpenAI");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);
//正常注册
{
    var kernel = Kernel.CreateBuilder().
      AddOpenAIChatCompletion(modelId: config.ModelId, apiKey: config.ApiKey)
        .Build();

}

//依赖注入
{
    IServiceCollection services = new ServiceCollection();
    //会话服务注册到IOC容器
    services.AddKernel().AddOpenAIChatCompletion(modelId: config.ModelId, apiKey: config.ApiKey, httpClient: client);
    services.AddSingleton<KernelPlugin>(sp => KernelPluginFactory.CreateFromType<LightPlugin>(serviceProvider: sp));
    var kernel = services.BuildServiceProvider().GetRequiredService<Kernel>();

    // Create chat history
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

}