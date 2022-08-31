namespace Loupedeck.Vlc
{
    using System;


    class JogAdjustment : PluginDynamicAdjustment
    {
        private Double _initialPosition = Vlc.InitialPosition;
        private readonly Vlc _vlcPlugin = new Vlc();

        public JogAdjustment() : base("Jog", "Scroll through track and play", "Playback navigation", false)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            this._initialPosition = Vlc.InitialPosition;
            if (ticks > 0)
            {
                if (this._initialPosition < Vlc.TrackLength)
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
            Vlc.InitialPosition = this._initialPosition;
            this.ActionImageChanged(actionParameter);
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Play();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width50.Rewind.png");

        protected override String GetAdjustmentValue(String actionParameter)
        {
            var time = TimeSpan.FromSeconds(this._initialPosition);
            var format = Vlc.TrackLength >= 3600 ? @"h\:mm\:ss" : @"mm\:ss";
            return time.ToString(format);
        }
    }

}
