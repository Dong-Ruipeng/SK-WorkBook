// See https://aka.ms/new-console-template for more information
using SkUtil.Configs;
using NativePlugins;

using SkUtil.CustomLLM;
using Microsoft.SemanticKernel;
using NativePlugins.Plugins.Weather;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using NativePlugins.Plugins.Finefood;
using Microsoft.Extensions.DependencyInjection;
var config = ConfigExtensions.FromConfig<OpenAIConfig>("InternalAzureOpenAI");

var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: config.ModelId,
apiKey: config.ApiKey).Build();
Console.WriteLine("ImportPluginFromType 创建插件");
{
    kernel.ImportPluginFromType<WeatherPlugin>();
    var getWeatherFunc = kernel.Plugins.GetFunction(nameof(WeatherPlugin), "WeatherSearch");
    var weatherContent = await getWeatherFunc.InvokeAsync(kernel, new() { ["city"] = "北京" });
    Console.WriteLine(weatherContent.ToString());
}


Console.WriteLine("ImportPluginFromObject");
{
    //FinefoodPlugin finefoodPlugin = new();

    //依赖注入举例

    IServiceCollection services = new ServiceCollection();
    services.AddSingleton<FinefoodPlugin>();
    var rootProvider = services.BuildServiceProvider();
    FinefoodPlugin finefoodPlugin = rootProvider.GetRequiredService<FinefoodPlugin>();
    kernel.ImportPluginFromObject(finefoodPlugin);
    var getFinefoodListFunc = kernel.Plugins.GetFunction(nameof(FinefoodPlugin), "GetFinefoodList");
    var weatherContent = await getFinefoodListFunc.InvokeAsync(kernel, new() { ["city"] = "北京" });
    Console.WriteLine(weatherContent.ToString());
}

Console.WriteLine("ImportPluginFromFunctions");
{
    var kernelfunction = kernel.CreateFunctionFromMethod((string city) => { return $"{city} 好玩的地方有八达岭长城，故宫，恭王府等"; },
        functionName: "GetTourismClassic", description: "获取城市的经典",
         [
            new KernelParameterMetadata(name:"city") {
             Description="城市名"
    }]);
    kernel.ImportPluginFromFunctions("TourismClassicPlugin", [kernelfunction]);
    var getTourismClassic = kernel.Plugins.GetFunction("TourismClassicPlugin", "GetTourismClassic");
    var weatherContent = await getTourismClassic.InvokeAsync(kernel, new() { ["city"] = "北京" });
    Console.WriteLine(weatherContent.ToString());
}


Console.ReadKey();