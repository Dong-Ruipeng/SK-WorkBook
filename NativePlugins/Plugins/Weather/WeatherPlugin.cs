using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace NativePlugins.Plugins.Weather;

public class WeatherPlugin
{
    [KernelFunction, Description("根据城市查询天气")]
    public string WeatherSearch([Description("城市名")]string city)
    {
        return $"{city}, 25℃，天气晴朗。";
    }
}
