using Microsoft.Extensions.Configuration;

namespace EAI.Core.Extensions;

public static class ConfigurationExtensions
{
    public static T GetSection<T>(this IConfiguration configuration, string sectionName) where T : class, new()
    {
        var section = configuration.GetSection(sectionName);
        var result = new T();
        section.Bind(result);
        return result;
    }
}
