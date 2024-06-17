using System.ComponentModel;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Text;

namespace SKPlugins.Core;

public class CustomConversationSummaryPlugin
{

    internal const string SummarizeConversationDefinition =
     @"BEGIN CONTENT TO SUMMARIZE:
{{$INPUT}}

END CONTENT TO SUMMARIZE.

Please summarize the conversation, highlighting the main points and any conclusions reached, in the language that best fits the content.
Do not incorporate any external general knowledge.
The summary should be in plain text, in complete sentences, without any markup or tags.

BEGIN SUMMARY:
";
    private const int MaxTokens = 1024;

    private readonly KernelFunction _summarizeConversationFunction;
    public CustomConversationSummaryPlugin()
    {


        PromptExecutionSettings settings = new()
        {
            ExtensionData = new Dictionary<string, object>()
            {
                { "Temperature", 0.1 },
                { "TopP", 0.5 },
                { "MaxTokens", MaxTokens }
            }
        };

        this._summarizeConversationFunction = KernelFunctionFactory.CreateFromPrompt(
            CustomConversationSummaryPlugin.SummarizeConversationDefinition,
            description: "Given a section of a conversation transcript, summarize the part of the conversation.",
            executionSettings: settings);
    }

    /// <summary>
    /// Given a long conversation transcript, summarize the conversation.
    /// </summary>
    /// <param name="input">A long conversation transcript.</param>
    /// <param name="kernel">The <see cref="Kernel"/> containing services, plugins, and other state for use throughout the operation.</param>
    [KernelFunction, Description("Given a long conversation transcript, summarize the conversation.")]
    public Task<string> SummarizeConversationAsync(
        [Description("A long conversation transcript.")] string input,
        Kernel kernel) =>
        ProcessAsync(this._summarizeConversationFunction, input, kernel);
    private static async Task<string> ProcessAsync(KernelFunction func, string input, Kernel kernel)
    {
        List<string> lines = TextChunker.SplitPlainTextLines(input, MaxTokens);
        List<string> paragraphs = TextChunker.SplitPlainTextParagraphs(lines, MaxTokens);

        string[] results = new string[paragraphs.Count];

        for (int i = 0; i < results.Length; i++)
        {
            // The first parameter is the input text.
            results[i] = (await func.InvokeAsync(kernel, new() { ["input"] = paragraphs[i] }).ConfigureAwait(false))
                .GetValue<string>() ?? string.Empty;
        }

        return string.Join("\n", results);
    }
}
