using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Pivotal.Web.Config.Transform.Buildpack;
using System.IO;
using Xunit;

namespace UnitTests
{
    public class WebConfigTransformBuildpackTests
    {
        IEnvironmentWrapper _fakeEnvironmentWrapper;
        IFileWrapper _fakeFileWrapper;
        IConfigurationFactory _fakeConfigurationFactory;
        IXmlDocumentWrapper _fakeWebConfigDocument;

        public WebConfigTransformBuildpackTests()
        {
            _fakeEnvironmentWrapper = A.Fake<IEnvironmentWrapper>();
            _fakeFileWrapper = A.Fake<IFileWrapper>();
            _fakeConfigurationFactory = A.Fake<IConfigurationFactory>();
            _fakeWebConfigDocument = A.Fake<IXmlDocumentWrapper>();

        }

        [Fact]
        public void WhenWebConfigIsNotFound()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, null, null);

            // act
            bp.Run(new[] { "supply", "path\\that\\does\\not\\exist", "", "", "0" });

            // assert
            A.CallTo(() => _fakeEnvironmentWrapper.Exit(0)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeWebConfigDocument.CreateDocFromFile(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void WhenASPNETCORE_ENVIRONMENTEnvVariableDefined()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, _fakeConfigurationFactory, _fakeWebConfigDocument);
            const string configPath = "example\\path";
            A.CallTo(() => _fakeFileWrapper.Exists(Path.Combine(configPath, "web.config"))).Returns(true);
            A.CallTo(() => _fakeEnvironmentWrapper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")).Returns("definedValue");

            // act
            bp.Run(new[] { "supply", configPath, "", "", "0" });

            // assert
            A.CallTo(() => _fakeConfigurationFactory.GetConfiguration("definedValue")).MustHaveHappenedOnceExactly();

        }

        [Fact]
        public void WhenASPNETCORE_ENVIRONMENTEnvVariableNotDefined()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, _fakeConfigurationFactory, _fakeWebConfigDocument);
            const string configPath = "example\\path";
            A.CallTo(() => _fakeFileWrapper.Exists(Path.Combine(configPath, "web.config"))).Returns(true);
            A.CallTo(() => _fakeEnvironmentWrapper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")).Returns(null);

            // act
            bp.Run(new[] { "supply", configPath, "", "", "0" });

            // assert
            A.CallTo(() => _fakeConfigurationFactory.GetConfiguration("Release")).MustHaveHappenedOnceExactly();

        }

        [Fact]
        public void WhenWebConfigOriginalFileAlreadyExists()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, _fakeConfigurationFactory, _fakeWebConfigDocument);
            const string configPath = "example\\path";
            const string configFileName = "web.config";
            string configFilepath = Path.Combine(configPath, configFileName);
            string origConfigFilePath = configFilepath + ".orig";


            A.CallTo(() => _fakeFileWrapper.Exists(configFilepath)).Returns(true);
            A.CallTo(() => _fakeFileWrapper.Exists(origConfigFilePath)).Returns(true);

            // act
            bp.Run(new[] { "supply", configPath, "", "", "0" });

            // assert
            A.CallTo(() => _fakeFileWrapper.Move(configFilepath, origConfigFilePath)).MustNotHaveHappened();

        }

        [Fact]
        public void WhenWebConfigOriginalFileDoesNotExist_BackupIsCreated()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, _fakeConfigurationFactory, _fakeWebConfigDocument);
            const string configPath = "example\\path";
            const string configFileName = "web.config";
            string configFilepath = Path.Combine(configPath, configFileName);
            string origConfigFilePath = configFilepath + ".orig";


            A.CallTo(() => _fakeFileWrapper.Exists(configFilepath)).Returns(true);
            A.CallTo(() => _fakeFileWrapper.Exists(origConfigFilePath)).Returns(false);

            // act
            bp.Run(new[] { "supply", configPath, "", "", "0" });

            // assert that backup is made
            A.CallTo(() => _fakeFileWrapper.Move(configFilepath, origConfigFilePath)).MustHaveHappenedOnceExactly();

        }


        [Fact]
        public void WhenWebConfigIsFound()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, _fakeConfigurationFactory, _fakeWebConfigDocument);
            const string configPath = "example\\path";
            A.CallTo(() => _fakeFileWrapper.Exists(Path.Combine(configPath, "web.config"))).Returns(true);

            // act
            bp.Run(new[] { "supply", configPath, "", "", "0" });

            // assert
            //A.CallTo(() => _fakeEnvironmentWrapper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")).MustHaveHappenedOnceExactly();
            //A.CallTo(() => _fakeConfigurationFactory.GetConfiguration(A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeWebConfigDocument.CreateDocFromFile(Path.Combine(configPath, "web.config"))).MustHaveHappenedOnceExactly();
        }
    }
}

/*Questions
1. If we the tests cover it, do we need a fake?
2. Do we need to assert that a call was made if it doesn't matter? 
3. Add to webConfigNotFound -- should we test that certain methods weren't called? 
4. DocumentWrapper - how to test 
5. Test by unit test, or by one happy path? (like filebackup)
 */