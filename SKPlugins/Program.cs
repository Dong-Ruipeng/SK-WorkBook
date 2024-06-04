// See https://aka.ms/new-console-template for more information

using SkUtil.Configs;
using SKPlugins;
using Microsoft.SemanticKernel;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("InternalAzureOpenAI");
//建议使用 OpenAI 的最新模型（如 gpt-3.5-turbo-1106 或 gpt-4-1106-preview）以获得最佳的工具调用体验。OpenAI 的最新模型通常具有更好的性能和更高的准确性，因此使用这些模型可以提高工具调用的效果。

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(config.ModelId, endpoint: config.Endpoint, apiKey: config.ApiKey)
    .Build();
//注册插件
string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
kernel.ImportPluginFromPromptDirectory(folder);

string[] pluginNames = ["Prompts", "WriterPlugins"];

foreach (var pluginName in pluginNames)
{
    kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, pluginName));
}

//测试从插件获得funciton
var jokeKernelFunction = kernel.Plugins.GetFunction("Prompts", "Translator");
Console.WriteLine("System: 请输入要翻译的内容");
var userResuest = Console.ReadLine();
Console.WriteLine("System: 请输入要翻译的语言语种");
var language = Console.ReadLine();

var results = await jokeKernelFunction.InvokeAsync(kernel, new KernelArguments()
{
  {"input", userResuest},
  {"language", language}
});

Console.WriteLine($"Assistant: {results.ToString()}");
Console.ReadKey();