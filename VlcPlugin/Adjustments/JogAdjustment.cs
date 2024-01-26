namespace Loupedeck.VlcPlugin
{
    using System;


    class JogAdjustment : PluginDynamicAdjustment
    {
        private Double _initialPosition = VlcPlugin.InitialPosition;
        private Double _initialLength = VlcPlugin.TrackLength;

        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public JogAdjustment() : base("Jog", "Scroll through track and play", "Playback navigation", false)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (this._vlcPlugin.TryGetTrackInfo(out var trackInfo))
            {
                if ((0 == this._initialPosition) || (trackInfo.TrackState.Time != this._initialPosition))
                {
                    this._initialPosition = trackInfo.TrackState.Time;
                }
                if (trackInfo.TrackState.Length != this._initialLength)
                {
                    this._initialLength = trackInfo.TrackState.Length;
                }
            }

            if (ticks > 0)
            {
                if (this._initialPosition < this._initialLength)
                {
                    this._initialPosition += 1;
                }
            }
            else
            {
                if (this._initialPosition > 0)
                {
                    this._initialPosition -= 1;
                }
            }
            this._vlcPlugin.Seek(this._initialPosition);
            VlcPlugin.InitialPosition = this._initialPosition;
            this.ActionImageChanged(actionParameter);
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Play();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.Rewind.png");

        protected override String GetAdjustmentValue(String actionParameter)
        {
            var time = TimeSpan.FromSeconds(this._initialPosition);
            var format = this._initialLength >= 3600 ? @"h\:mm\:ss" : @"mm\:ss";
            return time.ToString(format);
        }
    }

}
