using System;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    internal class EnvironmentWrapper : IEnvironmentWrapper
    {
        public void Exit(int code)
        {
            Environment.Exit(code);
        }
    }
}
