// See https://aka.ms/new-console-template for more information
using SkUtil.Configs;
using SkUtil.CustomLLM;
using PromptsFunctionsWithYaml;
using Microsoft.SemanticKernel;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("OneApiSpark");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);
//定义kernel 对象
var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: config.ModelId,
apiKey: config.ApiKey,
httpClient: client).Build();

//读取yaml文件地址
var yamlDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Yaml", "joke.yaml");
var promptYaml = await File.ReadAllTextAsync(yamlDirectory);
KernelFunction jokeFunc = kernel.CreateFunctionFromPromptYaml(promptYaml);

KernelArguments kernelArgs = new KernelArguments()
{
    {"topic","apple"},
    {"length","5"},

};
// 用内核调用函数并提供kernelArguments 
FunctionResult results = await jokeFunc.InvokeAsync(kernel, kernelArgs);

Console.WriteLine(results.ToString());

Console.ReadKey();