// See https://aka.ms/new-console-template for more information
using System.Text;

using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;

using SK_KernelMemory;

var config = ConfigExtensions.FromConfig<SkUtil.Configs.OpenAIConfig>("InternalAzureOpenAI");

///配置Kernel
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(config.ModelId, endpoint: config.Endpoint, apiKey: config.ApiKey)
    .AddAzureOpenAITextEmbeddingGeneration("text-embedding-3-large", endpoint: config.Endpoint, apiKey: config.ApiKey)
    .AddAzureOpenAITextGeneration("text-davinci-003", endpoint: config.Endpoint, apiKey: config.ApiKey)
    .Build();

//配置Kernel Memory
var textEmbeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
var textService = kernel.GetRequiredService<ITextGenerationService>();


var memoryBuilder = new KernelMemoryBuilder()
    .WithSemanticKernelTextEmbeddingGenerationService(textEmbeddingService, new Microsoft.KernelMemory.SemanticKernel.SemanticKernelConfig())
    .WithSemanticKernelTextGenerationService(textService, new Microsoft.KernelMemory.SemanticKernel.SemanticKernelConfig())
    .WithQdrantMemoryDb(new QdrantConfig()
    {
        Endpoint = "http://cnccappulv5030:6333"
    }).Build();

//var qdrantMemoryBuilder = new MemoryBuilder().
//WithTextEmbeddingGeneration(textEmbedding).
//WithQdrantMemoryStore("http://localhost:6333", 1536)
//.Build();


//插入数据
//await memoryBuilder.ImportDocumentAsync("《阿里云Kubernetes项目实战手册》.pdf", tags: new TagCollection()
//{
//    //指定集合
//    { "Collection","technical manual" }
//});

var chatService = kernel.GetRequiredService<IChatCompletionService>();

await ChatLoop(chatService, memoryBuilder);

static async Task ChatLoop(IChatCompletionService chatService, IKernelMemory memory)
{
    // Chat setup
    var systemPrompt = """
                           You are a helpful assistant replying to user questions using information from your memory.
                           Reply very briefly and concisely, get to the point immediately. Don't provide long explanations unless necessary.
                           Sometimes you don't have relevant memories so you reply saying you don't know, don't have the information.
                           The topic of the conversation is Kernel Memory (KM) and Semantic Kernel (SK).
                           """;

    var chatHistory = new ChatHistory(systemPrompt);

    // Start the chat
    var assistantMessage = "Hello, how can I help?";
    Console.WriteLine($"Copilot> {assistantMessage}\n");
    chatHistory.AddAssistantMessage(assistantMessage);

    // Infinite chat loop
    var reply = new StringBuilder();

    while (true)
    {
        // Get user message (retry if the user enters an empty string)
        Console.Write("You> ");
        var userMessage = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(userMessage)) { continue; }
        else { chatHistory.AddUserMessage(userMessage); }

        // Recall relevant information from memory
        var longTermMemory = await GetLongTermMemory(memory, userMessage);
        // Console.WriteLine("-------------------------- recall from memory\n{longTermMemory}\n--------------------------");

        // Inject the memory recall in the initial system message
        chatHistory[0].Content = $"{systemPrompt}\n\nLong term memory:\n{longTermMemory}";

        // Generate the next chat message, stream the response
        Console.Write("\nCopilot> ");
        reply.Clear();
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            MaxTokens = 2048
        };
        var result = await chatService.GetChatMessageContentAsync(chatHistory, openAIPromptExecutionSettings);

        Console.Write(result.Content);
        reply.Append(result.Content);


        chatHistory.AddAssistantMessage(reply.ToString());
        Console.WriteLine("\n");
    }
}

static async Task<string> GetLongTermMemory(IKernelMemory memory, string query, bool asChunks = true)
{
    if (asChunks)
    {
        // Fetch raw chunks, using KM indexes. More tokens to process with the chat history, but only one LLM request.
        SearchResult memories = await memory.SearchAsync(query, limit: 10);
        return memories.Results.SelectMany(m => m.Partitions).Aggregate("", (sum, chunk) => sum + chunk.Text + "\n").Trim();
    }

    // Use KM to generate an answer. Fewer tokens, but one extra LLM request.
    MemoryAnswer answer = await memory.AskAsync(query);
    return answer.Result.Trim();
}

