// See https://aka.ms/new-console-template for more information

// Create a kernel with OpenAI chat completion
using Microsoft.SemanticKernel;
using SkUtil.Configs;
using SK_CreateKernel;
using SkUtil.CustomLLM;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("OneApiSpark");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);
//Create Kernel
Kernel kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: config.ModelId,
        apiKey: config.ApiKey,
        httpClient: client)
    .Build();
// 接收用户入参
string request = Console.ReadLine()!;
// create prompt to the chat service
string prompt = "这个请求的意图是什么? {{$request}}";

// Create a kernel arguments object and add the  request
var kernelArguments = new KernelArguments
            {
                { "request", request }
            };
var streamingKernelContentsAsync = kernel.InvokePromptStreamingAsync(prompt, kernelArguments);
await foreach (var content in streamingKernelContentsAsync)
{
    Console.WriteLine(content);
}
Console.ReadKey();