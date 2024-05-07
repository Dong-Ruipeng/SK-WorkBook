
namespace SkUtil.CustomLLM;

public class OpenAICustomHandler : HttpClientHandler
{
    /// <summary>
    /// 用于OpenAI或Azure OpenAI请求时重定向的模型基础URL。
    /// </summary>
    private readonly string modelUrl;
    private static readonly string[] sourceArray = ["api.openai.com", "openai.azure.com"];

    /// <summary>
    /// 使用指定的模型URL初始化<see cref="OpenAICustomHandler"/>类的新实例。
    /// </summary>
    /// <param name="modelUrl">用于OpenAI或Azure OpenAI请求的基础URL。</param>
    public OpenAICustomHandler(string modelUrl)
    {
        // 确保modelUrl不是null或空
        if (string.IsNullOrWhiteSpace(modelUrl))
            throw new ArgumentException("模型URL不能为空或空白。", nameof(modelUrl));

        this.modelUrl = modelUrl;
    }

    /// <summary>
    /// 异步发送HTTP请求，对于OpenAI或Azure OpenAI服务的请求，将URL重定向到指定的模型URL。
    /// </summary>
    /// <param name="request">要发送的HTTP请求消息。</param>
    /// <param name="cancellationToken">可以用来取消操作的取消令牌。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 检查请求是否针对OpenAI或Azure OpenAI服务
        if (request.RequestUri != null &&
            (sourceArray.Contains(request.RequestUri.Host)))
        {         
            // 修改请求URI，以包含模型URL
            request.RequestUri = new Uri(modelUrl + request.RequestUri.PathAndQuery);
        }
        // 调用基类方法实际发送HTTP请求
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}