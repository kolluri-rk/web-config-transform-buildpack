using FakeItEasy;
using Pivotal.Web.Config.Transform.Buildpack;
using Xunit;

namespace UnitTests
{
    public class WebConfigTransformBuildpackTests
    {
        IEnvironmentWrapper _fakeEnvironmentWrapper;
        IBuildpackProcessor _fakeBuildpackProcessor;

        public WebConfigTransformBuildpackTests()
        {
            _fakeEnvironmentWrapper = A.Fake<IEnvironmentWrapper>();
            _fakeBuildpackProcessor = A.Fake<IBuildpackProcessor>();
        }

        [Fact]
        public void WhenWebConfigIsNotFound()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, null);

            // act
            bp.Run(new[] { "supply", "path\\that\\does\\not\\exist", "", "", "0" });

            // assert
            A.CallTo(() => _fakeEnvironmentWrapper.Exit(0)).MustHaveHappenedOnceExactly();
        }

        [Fact(Skip = "WIP")]
        public void WhenTransformationIsAppliedSuccessfully()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(null, _fakeBuildpackProcessor);

            // act
            bp.Run(new[] { "supply", "", "", "", "0" });

            // assert
            A.CallTo(() => _fakeBuildpackProcessor.ApplyTransformation()).MustHaveHappenedOnceExactly();
        }
    }
}