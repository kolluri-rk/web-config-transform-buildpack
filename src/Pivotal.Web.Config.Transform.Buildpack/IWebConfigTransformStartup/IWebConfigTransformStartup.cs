namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IWebConfigTransformStartup
    {
        void CopyExternalAppSettings(IWebConfigWriter webConfigWriter);

        void CopyExternalConnectionStrings(IWebConfigWriter webConfigWriter);

        void CopyExternalTokens(IWebConfigWriter webConfigWriter);
    }
}
