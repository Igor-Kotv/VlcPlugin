namespace Loupedeck.Vlc
{

    using System;

    class TogglePlayCommand : PluginDynamicCommand
    {
        private String _state;
        private readonly Vlc _vlcPlugin = new Vlc();

        public TogglePlayCommand() : base("Toggle Play", "Toggles play state", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._vlcPlugin.Play();

            var trackInfo = Vlc.GetTrackInfo();

            if (null != trackInfo)
            {
                this._state = trackInfo.TrackState.State;
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => this._state.IsNullOrEmpty() || (!this._state.IsNullOrEmpty() && this._state != "playing")
            ? EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.TogglePlay.png")
            : EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.Pause.png");
    }

}