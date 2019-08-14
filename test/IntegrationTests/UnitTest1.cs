using Pivotal.Web.Config.Transform.Buildpack;
using System;
using Xunit;

namespace IntegrationTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Environment.SetEnvironmentVariable("vcap:services:p-config-service", "{}");

            var bp = new WebConfigTransformBuildpack();

            bp.Run(new[] { "supply", "", "", "", "0" });
        }
    }
}
