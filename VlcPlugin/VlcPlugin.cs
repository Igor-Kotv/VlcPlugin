namespace Loupedeck.VlcPlugin
{
    using System;

    public partial class VlcPlugin : Plugin
    {
        private readonly PluginPreferenceAccount _vlcAccount;

        public VlcPlugin()
        {
            this._vlcAccount = new PluginPreferenceAccount("vlc-account")
            {
                DisplayName = "VLC",
                IsRequired = this.LoginRequired(),
                LoginUrlTitle = "Sign in to VLC",
                LogoutUrlTitle = "Sign out from VLC"
            };
        }

        public override void Load()
        {
            if (!this.ClientApplication.IsRunning())
            {
                this.SetPort();

                this.SetupVlc();
            }

            this.ConnectVlc();
            this.SetPluginIcons();

            this.ClientApplication.ApplicationStarted += this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped += this.OnApplicationStopped;
            this._vlcAccount.LoginRequested += this.OnVlcAccountOnLoginRequested;
            this._vlcAccount.LogoutRequested += this.OnVlcAccountOnLogoutRequested;

            this.ServiceEvents.UrlCallbackReceived += this.OnUrlCallbackReceived;
        }

        public override void Unload()
        {
            this.ClientApplication.ApplicationStarted -= this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped -= this.OnApplicationStopped;
            this.ServiceEvents.UrlCallbackReceived -= this.OnUrlCallbackReceived;
            this._vlcAccount.LoginRequested -= this.OnVlcAccountOnLoginRequested;
            this._vlcAccount.LogoutRequested -= this.OnVlcAccountOnLogoutRequested;
        }

        private void OnApplicationStarted(Object sender, EventArgs e) => this.ConnectVlc();

        private void OnApplicationStopped(Object sender, EventArgs e) => this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "VLC media player application is not running");

        private void OnUrlCallbackReceived(Object sender, UrlCallbackReceivedEventArgs e)
        {
            if (this.ClientApplication.IsRunning() && this.LoginRequired())
            {
                if ((e.Uri != null) && e.Uri.LocalPath.Contains("setPassword") && !String.IsNullOrEmpty(e.Uri.Query))
                {
                    this.SetPluginSetting("password", e.Uri.Query.Substring(1), false);
                    this.ConnectVlc();
                }
                else
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning, "Cannot connect to VLC application, please set a correct password");
                }
            }
        }

        private void OnVlcAccountOnLoginRequested(Object sender, EventArgs e)
        {
            if (!this.ClientApplication.IsRunning())
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Please start VLC media player application before signing in");
                return;
            }

            this.StartServer();
            this.OpenAuthenticationUrl();
        }

        private void OnVlcAccountOnLogoutRequested(Object sender, EventArgs e)
        {
            this.SetPluginSetting("password", "", false);
            this._vlcAccount.ReportLogout();
        }

        public void SetPluginIcons()
        {
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.metadata.Icon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.metadata.Icon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.metadata.Icon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.metadata.Icon256x256.png");
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
