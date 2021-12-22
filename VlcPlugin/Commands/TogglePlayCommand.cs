namespace Loupedeck.VlcPlugin
{

    using System;

    class TogglePlayCommand : PluginDynamicCommand
    {
        private String _state;
        public TogglePlayCommand() : base("Toggle Play", "Toggles play state", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            VlcPlugin.Play();

            var trackInfo = VlcPlugin.GetTrackInfo();

            if (null != trackInfo)
            {
                this._state = trackInfo.TrackState.State;
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => this._state.IsNullOrEmpty() || (!this._state.IsNullOrEmpty() && this._state != "playing")
            ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.TogglePlay.png")
            : EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.Pause.png");
    }

}