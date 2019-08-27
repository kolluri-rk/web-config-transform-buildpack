using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    class WebConfigTransformer
    {

        public void PerformTokenReplacements(string webConfig, Microsoft.Extensions.Configuration.IConfigurationRoot config)
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

        public XmlDocument ApplyConnectionStrings(XmlDocument doc, IConfigurationRoot config)
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
            return doc;
        }

        public XmlDocument ApplyAppSettings(XmlDocument doc, IConfigurationRoot config)
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
            return doc; 
        }

        public XmlDocument ApplyWebConfigTransform(string environment, string xdt, XmlDocument doc)
        {
            if (!string.IsNullOrEmpty(environment) && File.Exists(xdt))
            {
                Console.WriteLine($"-----> Applying {xdt} transform to web.config");
                var transform = new Microsoft.Web.XmlTransform.XmlTransformation(xdt);
                transform.Apply(doc);
            }
            return doc;
        }
    }
}
