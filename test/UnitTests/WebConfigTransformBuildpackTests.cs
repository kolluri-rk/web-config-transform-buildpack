using FakeItEasy;
using NUnit.Framework;
using Pivotal.Web.Config.Transform.Buildpack;

namespace UnitTests
{
    public class WebConfigTransformBuildpackTests
    {
        IEnvironmentWrapper _fakeEnvironmentWrapper;
        IBuildpackProcessor _fakeBuildpackProcessor;

        [SetUp]
        public void Setup()
        {
            _fakeEnvironmentWrapper = A.Fake<IEnvironmentWrapper>();
            _fakeBuildpackProcessor = A.Fake<IBuildpackProcessor>();
        }

        [Test]
        public void WhenWebConfigIsNotFound()
        {
            // arrange
            var bp = new WebConfigTransformBuildpack(_fakeEnvironmentWrapper, null);

            // act
            bp.Run(new[] { "supply", "path\\that\\does\\not\\exist", "", "", "0" });

            // assert
            A.CallTo(() => _fakeEnvironmentWrapper.Exit(0)).MustHaveHappenedOnceExactly();
        }

        [Test]
        [Ignore("WIP")]
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