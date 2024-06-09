using Microsoft.Extensions.Configuration;

namespace SK_KernelMemory;

public class ConfigExtensions
{
    public static T FromConfig<T>(string sectionName)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
               .Build();
        return configuration.GetSection(sectionName).Get<T>()
             ?? throw new InvalidDataException("Invalid semantic kernel configuration is empty");
    }
}
