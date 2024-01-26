namespace Loupedeck.VlcPlugin
{
    using System;

    class TogglePlayCommand : PluginMultistateDynamicCommand
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();
        private String _state = "";

        public TogglePlayCommand() : base("Toggle Play", "Play/pause media", "Playback")
        {
            this.AddState("paused", "Play", "Start playing");
            this.AddState("playing", "Pause", "Pause playing");
        }

        protected override void RunCommand(String actionParameter)
        {
            this._vlcPlugin.Play();

            var trackInfo = this._vlcPlugin.GetTrackInfo();

            if (null != trackInfo)
            {
                this._state = trackInfo.TrackState.State;
                if ((this._state == "playing" && this.GetCurrentState().Name == "paused") || (this._state == "paused" && this.GetCurrentState().Name == "playing"))
                {
                    this.ToggleCurrentState();
                }
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            var trackImage = this.States[stateIndex].Name != "playing" && (this._state.IsNullOrEmpty() || (!this._state.IsNullOrEmpty() && this._state != "playing"))
                ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.TogglePlay.png")
                : EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.Pause.png");

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