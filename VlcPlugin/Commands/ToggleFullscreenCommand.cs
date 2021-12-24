namespace Loupedeck.VlcPlugin
{

    using System;

    class ToggleFullscreenCommand : PluginDynamicCommand
    {
        private Boolean _fullscreen;
        public ToggleFullscreenCommand() : base("Toggle Fullscreen", "Toggles video playing on fullscreen", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            VlcPlugin.Fullscreen();

            var trackInfo = VlcPlugin.GetTrackInfo();

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