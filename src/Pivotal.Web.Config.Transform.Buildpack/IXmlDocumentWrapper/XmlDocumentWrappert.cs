using System;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class XmlDocumentWrapper : IXmlDocumentWrapper
    {
        public XmlDocument CreateDocFromFile(string filename)
        {
            var doc = new XmlDocument();
            doc.Load(filename);
            return doc;
        }
        public XmlDocument WriteFileFromDoc(XmlDocument doc, string filename)
        {
            doc.Save(filename);
            return doc; 
        }
    }
}
