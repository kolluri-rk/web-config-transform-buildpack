using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class Program
    {
        static int Main(string[] args)
        {

            // setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IEnvironmentWrapper, EnvironmentWrapper>()
                .AddSingleton<IFileWrapper, FileWrapper>()
                .AddSingleton<IConfigurationFactory, ConfigurationFactory>()
                .AddSingleton<WebConfigTransformBuildpack>()
                .BuildServiceProvider();


            // get services from DI container
            var buildpack = serviceProvider.GetService<WebConfigTransformBuildpack>();

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogInformation("Resolved services and starting buildpack");


            return buildpack.Run(args);
        }
    }
}