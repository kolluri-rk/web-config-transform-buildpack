using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IEnvironmentWrapper
    {
        void Exit(int code);
    }
}
