using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IXmlDocumentWrapper

    {
        XmlDocument CreateDocFromFile(string filename);
        XmlDocument WriteFileFromDoc(XmlDocument doc, string filename);


    }
}
