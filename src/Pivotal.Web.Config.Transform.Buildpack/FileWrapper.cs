using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class FileWrapper : IFileWrapper
    {
        public bool Exists(string file)
        {
            return System.IO.File.Exists(file);
        }
    }
}
