using Microsoft.Extensions.Configuration;

namespace AppCenterBuild.Utils
{
    internal static class Helper
    {
        public static IConfigurationRoot GetConfig()
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnvLoader.Load(dotenv);

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            return config;
        }
    }
}
