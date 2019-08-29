using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IXmlDocumentWrapper

    {
        //TODO: remove after buildpack is completely refactored
        XmlDocument CreateDocFromFile(string filename);

        XmlDocument CreateXmlDocFromFile(string filename);

        void SaveXmlDocAsFile(XmlDocument doc, string filename);

        string ConvertXmlDocToString(XmlDocument doc);

        XmlDocument CreateXmlDocFromString(string xmlData);
    }
}
