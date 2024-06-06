using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace NativePlugins.Plugins.Finefood;

public class FinefoodPlugin
{
    [KernelFunction, Description("根据城市获取美食推荐")]
    public string GetFinefoodList([Description("城市名")] string city)
    {
        return "烤鸭,卤煮，老北京炸酱面，炒肝等";
    }
}
