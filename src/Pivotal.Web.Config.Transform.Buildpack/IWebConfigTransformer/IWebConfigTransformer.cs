using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IWebConfigTransformer
    {
        XmlDocument ApplyWebConfigTransform(string environment, string xdt, XmlDocument doc);
        XmlDocument ApplyAppSettings(XmlDocument doc, IConfigurationRoot config);
        XmlDocument ApplyConnectionStrings(XmlDocument doc, IConfigurationRoot config);
        void PerformTokenReplacements(string webConfig, IConfigurationRoot config);

    }
}
