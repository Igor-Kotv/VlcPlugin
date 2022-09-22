namespace Loupedeck.Vlc
{

    using System;

    class ToggleRandomCommand : PluginDynamicCommand
    {
        private Boolean _random;
        private readonly Vlc _vlcPlugin = new Vlc();

        public ToggleRandomCommand() : base("Toggle Random", "Toggles random playing of tracks in playlist", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._vlcPlugin.ToggleRandom();

            var trackInfo = this._vlcPlugin.GetTrackInfo();

            if (null != trackInfo)
            {
                this._random = trackInfo.TrackState.Random;
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => this._random
            ? EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.RandomOn.png")
            : EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.RandomOff.png");
    }

}