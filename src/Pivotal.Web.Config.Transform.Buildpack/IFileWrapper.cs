using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IFileWrapper
    {
        bool Exists(string file);
    

    }
}
