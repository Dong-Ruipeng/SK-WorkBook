// See https://aka.ms/new-console-template for more information

using SkUtil.Configs;
using SkUtil.CustomLLM;
using Prompt_function;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Azure.Core;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("OneApiSpark");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);
string request = "I want to send an email to the marketing team celebrating their recent milestone.";
//Section 1

//基于String模版创建kernel functions
//Console.WriteLine("====>基于String模版创建kernel functions<=====");
//{
//    string prompt = "What is the intent of this request? {{$request}}";
//    var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: config.ModelId,
//        apiKey: config.ApiKey,
//        httpClient: client).Build();
//    var kernelFunction = kernel.CreateFunctionFromPrompt(prompt);


//    // Create a kernel arguments object and add the  request
//    var kernelArguments = new KernelArguments
//            {
//                { "request", request }
//            };
//    var functionResult = await kernelFunction.InvokeAsync(kernel, kernelArguments);

//    Console.WriteLine(functionResult?.ToString() ?? string.Empty);

//}

//// 基于基于String模版创建kernel functions ，SK自动创建Kernel functions

//{
//    string prompt = "What is the intent of this request? {{$request}}";
//    var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: config.ModelId,
//        apiKey: config.ApiKey,
//        httpClient: client).Build();



//    // Create a kernel arguments object and add the  request
//    var kernelArguments = new KernelArguments
//            {
//                { "request", request }
//            };
//    var functionResult = await kernel.InvokePromptAsync(prompt, kernelArguments);

//    Console.WriteLine(functionResult?.ToString() ?? string.Empty);

//}


// 基于PromptTemplateConfig 创建 kernel functions
Console.WriteLine("====>基于PromptTemplateConfig 创建 kernel functions<=====");

{
    var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: config.ModelId,
    apiKey: config.ApiKey,
    httpClient: client).Build();

    var kernelFunctions = kernel.CreateFunctionFromPrompt(new PromptTemplateConfig()
    {
        Name = "intent",
        Description = "use assistant to understand user input intent.",
        TemplateFormat = PromptTemplateConfig.SemanticKernelTemplateFormat,//此处可以省略默认就是"semantic-kernel"
        Template = "What is the intent of this request? {{$request}}",
        InputVariables = [new() { Name = "request", Description = "The user's request.", IsRequired = true }],
        ExecutionSettings = new Dictionary<string, PromptExecutionSettings>() {
               {
                      OpenAIPromptExecutionSettings.DefaultServiceId ,//"default"
                        new OpenAIPromptExecutionSettings()
                        {
                            MaxTokens = 1024,
                            Temperature = 0
                        }
                    },
        }
    });
    var kernelArguments = new KernelArguments
    {
                { "request", request }
            };
    var functionResult = await kernelFunctions.InvokeAsync(kernel, kernelArguments);

    Console.WriteLine(functionResult?.ToString() ?? string.Empty);
}
Console.ReadKey();