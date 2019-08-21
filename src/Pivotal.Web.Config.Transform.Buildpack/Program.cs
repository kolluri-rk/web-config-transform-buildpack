using System;
using Microsoft.Extensions.DependencyInjection;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class Program
    {
        static int Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEnvironmentWrapper, EnvironmentWrapper>()
                .AddSingleton<IFileWrapper, FileWrapper>()
                .AddSingleton<IConfigurationFactory, ConfigurationFactory>()
                .AddSingleton<WebConfigTransformBuildpack>()
                .BuildServiceProvider();

            var buildpack = serviceProvider.GetService<WebConfigTransformBuildpack>();
            return buildpack.Run(args);

            //return new WebConfigTransformBuildpack(new EnvironmentWrapper(), new FileWrapper(), new ConfigurationFactory()).Run(args);
        }
    }
}