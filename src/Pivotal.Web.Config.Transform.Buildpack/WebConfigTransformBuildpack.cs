﻿using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class WebConfigTransformBuildpack : SupplyBuildpack
    {
        
        protected override bool Detect(string buildPath)
        {
            return false;
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("=============== WebConfig Transform Buildpack execution started ================");
            Console.WriteLine("================================================================================");

            var webConfig = Path.Combine(buildPath, "web.config");

            if (!File.Exists(webConfig))
            {
                Console.WriteLine("-----> Web.config not detected, skipping further execution");
                Environment.Exit(0);
            }

            ApplyTransformations(buildPath, webConfig);

            Console.WriteLine("================================================================================");
            Console.WriteLine("============== WebConfig Transform Buildpack execution completed ===============");
            Console.WriteLine("================================================================================");
        }

        private static void ApplyTransformations(string buildPath, string webConfig)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";

            Console.WriteLine($"-----> Using Environment: {environment}");

            var config = GetConfiguration(environment);

            var xdt = Path.Combine(buildPath, $"web.{environment}.config");
            var doc = new XmlDocument();
            doc.Load(webConfig);

            if (!File.Exists(webConfig + ".orig")) // backup original web.config as we're gonna transform into it's place
                File.Move(webConfig, webConfig + ".orig");
            doc.Save(webConfig);

            ApplyWebConfigTransform(environment, xdt, doc);
            ApplyAppSettings(doc, config);
            ApplyConnectionStrings(doc, config);
            PerformTokenReplacements(webConfig, config);
        }

        private static IConfigurationRoot GetConfiguration(string environment)
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddEnvironmentVariables();

            if (IsConfigServerBound())
            {
                Console.WriteLine("-----> Config server binding found...");

                configBuilder.AddConfigServer(environment,
#pragma warning disable CS0618 // Type or member is obsolete
                    new LoggerFactory(new[] { new ConsoleLoggerProvider((name, level) => { level = LogLevel.Information; return true; }, false) }));
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
                configBuilder.AddCloudFoundry();

            return configBuilder.Build();
        }

        private static void PerformTokenReplacements(string webConfig, IConfigurationRoot config)
        {
            var webConfigContent = File.ReadAllText(webConfig);
            foreach (var configEntry in config.AsEnumerable())
            {
                var replaceToken = "#{" + configEntry.Key + "}";

                if (webConfigContent.Contains(replaceToken))
                {
                    Console.WriteLine($"-----> Replacing token `{replaceToken}` in web.config");
                    webConfigContent = webConfigContent.Replace(replaceToken, configEntry.Value);
                }
            }
            File.WriteAllText(webConfig, webConfigContent);
        }

        private static void ApplyConnectionStrings(XmlDocument doc, IConfigurationRoot config)
        {
            var connStr = doc.SelectNodes("/configuration/connectionStrings/add").OfType<XmlElement>();

            foreach (var add in connStr)
            {
                var key = add.GetAttribute("name");

                if (key == null)
                    continue;

                var value = config[key];

                if (!string.IsNullOrEmpty(value))
                {
                    Console.WriteLine($"-----> Replacing connectionString for matching connectionString name `{key}` in web.config");
                    add.SetAttribute("connectionString", value);
                }
            }
        }

        private static void ApplyAppSettings(XmlDocument doc, IConfigurationRoot config)
        {
            var adds = doc.SelectNodes("/configuration/appSettings/add").OfType<XmlElement>();

            foreach (var add in adds)
            {
                var key = add.GetAttribute("key");
                if (key == null)
                    continue;

                var value = config[key];

                if (!string.IsNullOrEmpty(value))
                {
                    Console.WriteLine($"-----> Replacing value for matching appSetting key `{key}` in web.config");
                    add.SetAttribute("value", value);
                }
            }
        }

        private static void ApplyWebConfigTransform(string environment, string xdt, XmlDocument doc)
        {
            if (!string.IsNullOrEmpty(environment) && File.Exists(xdt))
            {
                Console.WriteLine($"-----> Applying {xdt} transform to web.config");
                var transform = new Microsoft.Web.XmlTransform.XmlTransformation(xdt);
                transform.Apply(doc);
            }
        }

        private static bool IsConfigServerBound()
        {
            var configuration = new ConfigurationBuilder()
                                        .AddCloudFoundry()
                                        .Build();

            var cfServicesOptions = new CloudFoundryServicesOptions(configuration);

            foreach (var service in cfServicesOptions.ServicesList)
            {
                if (service.Label == "p-config-server" 
                    || service.Label == "p.config-server" 
                    || (service.Tags != null && (service.Tags.Contains("spring-cloud") && service.Tags.Contains("configuration"))))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
