namespace SkUtil.Configs;

/// <summary>
/// 模型自定义配置 
/// </summary>
public class OpenAIConfig
{
    public string Endpoint { get; set; } = null!;
    public string ModelId { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
}
