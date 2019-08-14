using Pivotal.Web.Config.Transform.Buildpack;
using System;
using System.IO;
using System.Xml;
using Xunit;

namespace IntegrationTests
{
    public class WebConfigTransformBuildpackTests : IDisposable
    {
        public WebConfigTransformBuildpackTests()
        {
        }

        public void Dispose()
        {
            File.Delete("web.config.orig");
        }

        [Fact]
        public void WhenWebConfigIsTransformedSuccessfully()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack();

            // act
            bp.Run(new[] { "supply", "", "", "", "0" });

            // assert
            var xml = new XmlDocument();
            xml.Load("web.config");

            var transformedValue = xml.SelectSingleNode("/configuration/qux/quz");

            Assert.NotNull(transformedValue);
        }

        [Fact]
        public void WhenAppSettingsAreChangedSuccessfully()
        {
            // arrange
            const string expectedValue = "BP_AppSettings_Value1";

            Environment.SetEnvironmentVariable("BP_AppSettings_Key1", expectedValue);

            var bp = new WebConfigTransformBuildpack();

            // act
            bp.Run(new[] { "supply", "", "", "", "0" });

            // assert
            var xml = new XmlDocument();
            xml.Load("web.config");

            var actualValue = xml.SelectSingleNode("/configuration/appSettings/add[@key='BP_AppSettings_Key1']/@value").Value;

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WhenConnectionStringsAreChangedSuccessfully()
        {
            // arrange
            const string expectedValue = "BP_ConnectionStrings_Value1";

            Environment.SetEnvironmentVariable("BP_ConnectionStrings_Key1", expectedValue);

            var bp = new WebConfigTransformBuildpack();

            // act
            bp.Run(new[] { "supply", "", "", "", "0" });

            // assert
            var xml = new XmlDocument();
            xml.Load("web.config");

            var actualValue = xml.SelectSingleNode("/configuration/connectionStrings/add[@name='BP_ConnectionStrings_Key1']/@connectionString").Value;

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WhenTokenizedValueIsChangedSuccessfully()
        {
            // arrange
            const string expectedValue = "BP_Value1";

            Environment.SetEnvironmentVariable("BP_Token1", expectedValue);

            var bp = new WebConfigTransformBuildpack();

            // act
            bp.Run(new[] { "supply", "", "", "", "0" });

            // assert
            var xml = new XmlDocument();
            xml.Load("web.config");

            var actualValue = xml.SelectSingleNode("/configuration/foo/bar/@baz").Value;

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void VerifyWebConfigBackupIsCreated()
        {
            //arrange
            var bp = new WebConfigTransformBuildpack();

            //act
            bp.Run(new[] { "supply", "", "", "", "0" });


            //assert 
            Assert.True(File.Exists("web.config.orig"));

        }
    }
}
