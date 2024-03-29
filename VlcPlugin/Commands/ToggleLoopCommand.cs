﻿namespace Loupedeck.VlcPlugin
{

    using System;

    class ToggleLoopCommand : PluginDynamicCommand
    {
        private Boolean _loopState;
        private Boolean _repeatState;
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public ToggleLoopCommand() : base("Toggle Loop", "Toggles repeat playlist or one track", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._vlcPlugin.ToggleLoop(this._loopState, this._repeatState);

            var trackInfo = this._vlcPlugin.GetTrackInfo();

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
                ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.LoopOn.png")
                : this._repeatState
                ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.RepeatOne.png")
                : EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.Loop.png");
        }


    }

}

