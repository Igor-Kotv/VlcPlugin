namespace Loupedeck.Vlc
{
    using System;

    class TogglePlayCommand : PluginMultistateDynamicCommand
    {
        private String _state;
        private readonly Vlc _vlcPlugin = new Vlc();

        public TogglePlayCommand() : base("Toggle Play", "Toggles play state", "Playback")
        {
            this.AddState("paused", "Play", "Start playing");
            this.AddState("playing", "Pause", "Pause playing");
        }

        protected override void RunCommand(String actionParameter)
        {
            this._vlcPlugin.Play();
            this.ToggleCurrentState();

            var trackInfo = this._vlcPlugin.GetTrackInfo();

            if (null != trackInfo)
            {
                this._state = trackInfo.TrackState.State;
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            var trackImage = this.GetCurrentState().Name != "playing" && (this._state.IsNullOrEmpty() || (!this._state.IsNullOrEmpty() && this._state != "playing"))
                ? EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.TogglePlay.png")
                : EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.Pause.png");

            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                if (this._vlcPlugin.TryGetCoverArt(imageSize, out var coverArt))
                {
                    bitmapBuilder.DrawImage(coverArt);
                }
                bitmapBuilder.DrawImage(trackImage);
                return bitmapBuilder.ToImage();
            }
        }
    }

}