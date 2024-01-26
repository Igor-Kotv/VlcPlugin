namespace Loupedeck.VlcPlugin
{
    using System;
    using System.IO;

    public partial class VlcPlugin : Plugin
    {
        public override Boolean Install()
        {
            var pluginDataDirectory = this.GetPluginDataDirectory();
            if (!IoHelpers.EnsureDirectoryExists(pluginDataDirectory))
            {
                Tracer.Error("Plugin data is not created. Cannot continue installation");
                return false;
            }

            var filePath = Path.Combine(pluginDataDirectory, "AuthorizationPage.html");

            this.Assembly.ExtractFile("Loupedeck.VlcPlugin.Resources.AuthorizationPage.html", filePath);

            return true;
        }

        public void CopyPluginData()
        {
            var pluginDataDirectory = this.GetPluginDataDirectory();
            var filePath = Path.Combine(pluginDataDirectory, "AuthorizationPage.html");
            this.Assembly.ExtractFile("Loupedeck.VlcPlugin.Resources.AuthorizationPage.html", filePath);
        }
    }
}
