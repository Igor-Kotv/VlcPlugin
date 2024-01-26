namespace Loupedeck.VlcPlugin
{

    using System;

    class ToggleFullscreenCommand : PluginDynamicCommand
    {
        private Boolean _fullscreen;
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public ToggleFullscreenCommand() : base("Toggle Fullscreen", "Toggles video playing on fullscreen", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._vlcPlugin.Fullscreen();

            var trackInfo = this._vlcPlugin.GetTrackInfo();

            if (null != trackInfo)
            {
                this._fullscreen = trackInfo.TrackState.Fullscreen;
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => this._fullscreen
            ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.FullScreen.png")
            : EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.ExitFullScreen.png");
    }

}