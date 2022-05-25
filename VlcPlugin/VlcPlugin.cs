namespace Loupedeck.VlcPlugin
{
    using System;

    public partial class VlcPlugin : Plugin
    {
        public override void Load()
        {
            this.ConnectVlc();
            this.SetPluginIcons();
            this.CopyPluginData();

            this.ClientApplication.ApplicationStarted += this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped += this.OnApplicationStopped;

            this.ServiceEvents.UrlCallbackReceived += this.OnUrlCallbackReceived;

        }

        public override void Unload()
        {
            this.ClientApplication.ApplicationStarted -= this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped -= this.OnApplicationStopped;
            this.ServiceEvents.UrlCallbackReceived -= this.OnUrlCallbackReceived;
        }

        private void OnApplicationStarted(Object sender, EventArgs e) => this.ConnectVlc();

        private void OnApplicationStopped(Object sender, EventArgs e) => this.ConnectVlc();

        private void OnUrlCallbackReceived(Object sender, UrlCallbackReceivedEventArgs e)
        {
            if ((e.Uri != null) && e.Uri.LocalPath.Contains("setPassword") && !String.IsNullOrEmpty(e.Uri.Query))
            {
                this.SetPluginSetting("password", e.Uri.Query.Substring(1), false);
                this.ConnectVlc();
            }
        }

        public void SetPluginIcons()
        {
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.PluginIcon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.PluginIcon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.PluginIcon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.PluginIcon256x256.png");
        }

        public override void RunCommand(String commandName, String parameter)
        {
        }

        public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff)
        {
        }


        public override Boolean HasNoApplication => true;
        public override Boolean UsesApplicationApiOnly => true;
    }
}
