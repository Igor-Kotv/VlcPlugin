namespace Loupedeck.Vlc
{

    using System;

    class ToggleLoopCommand : PluginDynamicCommand
    {
        private Boolean _loopState;
        private Boolean _repeatState;
        private readonly Vlc _vlcPlugin = new Vlc();

        public ToggleLoopCommand() : base("Toggle Loop", "Toggles repeat playlist or one track", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._vlcPlugin.ToggleLoop(this._loopState, this._repeatState);

            var trackInfo = Vlc.GetTrackInfo();

            if (null != trackInfo)
            {
                this._loopState = trackInfo.TrackState.Loop;
                this._repeatState = trackInfo.TrackState.Repeat;
                this.ActionImageChanged(actionParameter);
            }
        }
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._loopState
                ? EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.LoopOn.png")
                : this._repeatState
                ? EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.RepeatOne.png")
                : EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.Loop.png");
        }


    }

}

