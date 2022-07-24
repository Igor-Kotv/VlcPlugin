namespace Loupedeck.Vlc
{
    using System;
    using System.IO;

    public partial class Vlc : Plugin
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

            this.Assembly.ExtractFile("Loupedeck.Vlc.Resources.AuthorizationPage.html", filePath);

            return true;
        }

        public void CopyPluginData()
        {
            var pluginDataDirectory = this.GetPluginDataDirectory();
            var filePath = Path.Combine(pluginDataDirectory, "AuthorizationPage.html");
            this.Assembly.ExtractFile("Loupedeck.Vlc.Resources.AuthorizationPage.html", filePath);
        }

    }
}
