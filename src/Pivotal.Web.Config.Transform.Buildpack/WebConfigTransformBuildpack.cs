﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class WebConfigTransformBuildpack : SupplyBuildpack
    {
        IEnvironmentWrapper _environmentWrapper;
        IFileWrapper _fileWrapper;
        IConfigurationFactory _configurationFactory;
        IXmlDocumentWrapper _xmlDocumentWrapper;

        public WebConfigTransformBuildpack(IEnvironmentWrapper environmentWrapper, IFileWrapper fileWrapper, IConfigurationFactory configurationFactory, IXmlDocumentWrapper xmlDocumentWrapper)
        {
            _environmentWrapper = environmentWrapper;
            _fileWrapper = fileWrapper;
            _configurationFactory = configurationFactory;
            _xmlDocumentWrapper = xmlDocumentWrapper;
        }
        
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

            if (_fileWrapper.Exists(webConfig))
            {
                ApplyTransformations(buildPath, webConfig);

                Console.WriteLine("================================================================================");
                Console.WriteLine("============== WebConfig Transform Buildpack execution completed ===============");
                Console.WriteLine("================================================================================");
                return;
            }

            Console.WriteLine("-----> Web.config not detected, skipping further execution");
            _environmentWrapper.Exit(0);
        }

        private void ApplyTransformations(string buildPath, string webConfig)
        {
            var environment = _environmentWrapper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";

            Console.WriteLine($"-----> Using Environment: {environment}");

            var config = _configurationFactory.GetConfiguration(environment);

            var doc = _xmlDocumentWrapper.CreateDocFromFile(webConfig);
            var xdt = Path.Combine(buildPath, $"web.{environment}.config");


            if (!_fileWrapper.Exists(webConfig + ".orig")) // backup original web.config as we're gonna transform into it's place
                _fileWrapper.Move(webConfig, webConfig + ".orig");

            ApplyWebConfigTransform(environment, xdt, doc);
            ApplyAppSettings(doc, config);
            ApplyConnectionStrings(doc, config);
            doc.Save(webConfig);
            PerformTokenReplacements(webConfig, config);
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

                var value = config[$"connectionStrings:{key}"];

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

                var value = config[$"appSettings:{key}"];

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

    }
}
