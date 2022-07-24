namespace Loupedeck.Vlc
{
    using System;


    class JogAdjustment : PluginDynamicAdjustment
    {
        private Boolean _forward = true;
        private Double _initialPosition = Vlc.InitialPosition;
        private readonly Vlc _vlcPlugin = new Vlc();

        public JogAdjustment() : base("Jog", "Scroll through track", "Playback", true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (ticks > 0)
            {
                this._forward = true;
                if (this._initialPosition < Vlc.TrackLength)
                {
                    this._initialPosition += 1;
                }
            }
            else
            {
                this._forward = false;
                if (this._initialPosition > 0)
                {
                    this._initialPosition -= 1;
                }
            }
            this._vlcPlugin.Seek(this._initialPosition);
            this.ActionImageChanged(actionParameter);
        }

        protected override void RunCommand(String actionParameter)
        {
            this._initialPosition = 0;
            this._vlcPlugin.Seek(this._initialPosition);
            this.ActionImageChanged(actionParameter);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._forward
                ? EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width50.JogForward50.png")
                : EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width50.JogBackwards50.png");
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {
            var time = TimeSpan.FromSeconds(this._initialPosition);
            var format = Vlc.TrackLength >= 3600 ? @"h\:mm\:ss" : @"mm\:ss";
            return time.ToString(format);
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) => "Back to the start";
    }

}
