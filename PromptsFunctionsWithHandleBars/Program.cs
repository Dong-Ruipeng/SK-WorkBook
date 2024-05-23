// See https://aka.ms/new-console-template for more information

using System;
using System.Data;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using PromptsFunctionsWithHandleBars;
using SkUtil.Configs;
using SkUtil.CustomLLM;

var config = ConfigExtensions.FromConfig<OpenAIConfig>("OneApiSpark");
//自定义HttpClientHandler
var openAICustomHandler = new OpenAICustomHandler(config.Endpoint);
using HttpClient client = new(openAICustomHandler);

//定义kernel 对象
var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: config.ModelId,
apiKey: config.ApiKey,
httpClient: client).Build();

//创建HandlerBars
var template =
            """
            <message role="system">Instructions: What is the intent of this request?</message>
          
            <message role="user">{{request}}</message>
            """;
//{
//    {#each chatHistory}}
//                < message role = "{{role}}" >{ { Content} }</ message >
//            { {/ each} }
//var template = "What is the intent of this request? {{request}}";
var kernelFunction = kernel.CreateFunctionFromPrompt(new PromptTemplateConfig()
{
    Name = "getIntent",
    Description = "Understand the user's input intent.",
    TemplateFormat = HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
    Template = template,
    InputVariables = [
 new() { Name = "request", Description = "User's request.", IsRequired = true },
          //new() { Name = "history", Description = "Historical message record.", IsRequired = true },
        ],
    ExecutionSettings = new Dictionary<string, PromptExecutionSettings>() {
               {
                      OpenAIPromptExecutionSettings.DefaultServiceId ,//"default"
                        new OpenAIPromptExecutionSettings()
                        {
                            MaxTokens = 2048,
                            Temperature = 0.6
                        }
                    },
        }
}, promptTemplateFactory: new HandlebarsPromptTemplateFactory());


string request = "I want to send an email to the marketing team celebrating their recent milestone.";


var result = await kernelFunction.InvokeAsync(kernel, new KernelArguments() { { "request", request } });
Console.WriteLine(result.ToString());

Console.ReadKey();