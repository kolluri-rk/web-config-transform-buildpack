using Pivotal.Web.Config.Transform.Buildpack;
using System;
using System.IO;
using System.Xml;
using Xunit;

namespace IntegrationTests
{
    public class WebConfigTransformBuildpackTests : IDisposable
    {

        private readonly IEnvironmentWrapper _environmentWrapper;
        private readonly IFileWrapper _fileWrapper;
        private readonly IConfigurationFactory _configurationFactory;

        private readonly WebConfigTransformBuildpack _bp;

        public WebConfigTransformBuildpackTests()
        {
            _environmentWrapper = new EnvironmentWrapper();
            _fileWrapper = new FileWrapper();
            _configurationFactory = new ConfigurationFactory();

            _bp = new WebConfigTransformBuildpack(_environmentWrapper, _fileWrapper, _configurationFactory);


        }

        public void Dispose()
        {
            //File.Delete("web.config.orig");
            if (File.Exists("web.config.orig"))
            {
                File.Copy("web.config.orig", "web.config", true);
                File.Delete("web.config.orig");
            }
        }

        [Fact]
        public void WhenWebConfigIsTransformedSuccessfully()
        {
            // arrange
            //var bp = new WebConfigTransformBuildpack(_environmentWrapper, _fileWrapper, _configurationFactory);

            // act
            _bp.Run(new[] { "supply", "", "", "", "0" });

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
            const string expectedValue = "BP_AppSettings_Value123";

            Environment.SetEnvironmentVariable("appSettings:BP_AppSettings_Key1", expectedValue);


            // act
            _bp.Run(new[] { "supply", "", "", "", "0" });

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

            Environment.SetEnvironmentVariable("connectionStrings:BP_ConnectionStrings_Key1", expectedValue);


            // act
            _bp.Run(new[] { "supply", "", "", "", "0" });

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


            // act
            _bp.Run(new[] { "supply", "", "", "", "0" });

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
            //var bp = new WebConfigTransformBuildpack();

            //act
            _bp.Run(new[] { "supply", "", "", "", "0" });


            //assert 
            Assert.True(File.Exists("web.config.orig"));

        }
    }
}
