using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SkUtil.Configs;

namespace PromptsFunctionsWithYaml;

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
