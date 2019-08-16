using System;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    internal class EnvironmentWrapper : IEnvironmentWrapper
    {
        public void Exit(int code)
        {
            Environment.Exit(code);
        }

        public string GetEnvironmentVariable(string variable)
        {
            throw new NotImplementedException();
        }
    }
}
