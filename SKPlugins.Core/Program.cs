// See https://aka.ms/new-console-template for more information
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

using SKPlugins.Core;

using SkUtil.Configs;
using SkUtil.CustomLLM;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("InternalAzureOpenAI");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);

var kernel = Kernel.CreateBuilder().
   AddOpenAIChatCompletion(modelId: config.ModelId, apiKey: config.ApiKey, httpClient: client)
    .Build();


var conversationSummaryPlugin = kernel.ImportPluginFromType<ConversationSummaryPlugin>();


string chatTranscript = @"
A: 你好，最近工作很忙碌，我们需要安排下周的会议时间，你觉得周几比较合适？
B: 嗯，我明白，工作确实很忙。周三或周四应该比较合适，因为那时候大家的日程相对空闲一些。
A: 好的，周三或周四都可以，我们再确认一下其他同事的时间表。
B: 对，最好再和大家核实一下，免得出现时间冲突。
A: 我今天会发邮件询问大家的意见，然后我们再做最终决定。
B: 好的，我也会在群里提醒大家留意邮件。

A: 大家好，关于下周的会议安排，我建议定在周四下午两点，在会议室A举行，大家觉得怎么样？
C: 周四下午两点可以，我在日历上已经标注了。
D: 对不起，周四下午我有其他安排，能否改到周三下午呢？
A: 好的，我们尽量照顾大家的时间，那就改到周三下午两点吧，地点仍然是会议室A。
B: 没问题，我会通知其他同事，让大家知道时间的变动。

";


Console.WriteLine("SamplePlugins - Conversation Summary Plugin - Summarize");
{

    FunctionResult summary = await kernel.InvokeAsync(
        conversationSummaryPlugin["SummarizeConversation"], new() { ["input"] = chatTranscript });

    Console.WriteLine($"Generated Summary:{summary.ToString()}");
}

Console.WriteLine("-----------");





Console.WriteLine("======== SamplePlugins - Conversation Summary Plugin - Action Items ========");
{

    FunctionResult summary = await kernel.InvokeAsync(
        conversationSummaryPlugin["GetConversationActionItems"], new() { ["input"] = chatTranscript });

    Console.WriteLine($"Generated Action Items:{summary.ToString()}");
    Console.WriteLine(summary.GetValue<string>());

}




Console.WriteLine("======== SamplePlugins - Conversation Summary Plugin - Action Items ========");
{
    Console.WriteLine("======== SamplePlugins - Conversation Summary Plugin - Topics ========");

    FunctionResult summary = await kernel.InvokeAsync(
        conversationSummaryPlugin["GetConversationTopics"], new() { ["input"] = chatTranscript });

    Console.WriteLine("Generated Topics:");
    Console.WriteLine(summary.GetValue<string>());

}




Console.ReadKey();