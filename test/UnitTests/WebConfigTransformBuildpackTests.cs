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

        public WebConfigTransformBuildpackTests()
        {
            _fakeEnvironmentWrapper = A.Fake<IEnvironmentWrapper>();
            _fakeFileWrapper = A.Fake<IFileWrapper>();
            _fakeConfigurationFactory = A.Fake<IConfigurationFactory>();

        }

        [Fact]
        public void WhenWebConfigIsNotFound()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, null);

            // act
            bp.Run(new[] { "supply", "path\\that\\does\\not\\exist", "", "", "0" });

            // assert
            A.CallTo(() => _fakeEnvironmentWrapper.Exit(0)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WhenWebConfigIsFound()
        {
            // arrange
            const string configPath = "path\\that\\does\\exist";

            A.CallTo(() => _fakeFileWrapper.Exists(Path.Combine(configPath, "web.config"))).Returns(true);

            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, _fakeFileWrapper, _fakeConfigurationFactory);

            // act
            bp.Run(new[] { "supply", configPath, "", "", "0" });

            // assert
            A.CallTo(() => _fakeEnvironmentWrapper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeConfigurationFactory.GetConfiguration(A<string>._)).MustHaveHappenedOnceExactly();
        }
    }
}