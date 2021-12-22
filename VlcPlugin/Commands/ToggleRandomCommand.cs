namespace Loupedeck.VlcPlugin
{

    using System;

    class ToggleRandomCommand : PluginDynamicCommand
    {
        private Boolean _random;
        public ToggleRandomCommand() : base("Toggle Random", "Toggles random playing of tracks in playlist", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            VlcPlugin.ToggleRandom();

            var trackInfo = VlcPlugin.GetTrackInfo();

            if (null != trackInfo)
            {
                this._random = trackInfo.TrackState.Random;
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => this._random
            ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.RandomOn.png")
            : EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.RandomOff.png");
    }

}