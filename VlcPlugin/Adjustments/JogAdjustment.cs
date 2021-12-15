namespace Loupedeck.VlcPlugin
{
    using System;


    class JogAdjustment : PluginDynamicAdjustment
    {
        private Boolean _forward = true;
        private Double _initialPosition = VlcPlugin.InitialPosition;

        public JogAdjustment() : base("Jog", "Scroll through track", "Playback", true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (ticks > 0)
            {
                this._forward = true;
                if (this._initialPosition < VlcPlugin.TrackLength)
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
            VlcPlugin.Seek(this._initialPosition);
            this.ActionImageChanged(actionParameter);
        }

        protected override void RunCommand(String actionParameter)
        {
            this._initialPosition = 0;
            VlcPlugin.Seek(this._initialPosition);
            this.ActionImageChanged(actionParameter);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._forward
                ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.JogForward50.png")
                : EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.JogBackwards50.png");
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {

            var time = TimeSpan.FromSeconds(this._initialPosition);
            return time.ToString(@"h\:mm\:ss");
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) => "Back to the start";
    }

}
