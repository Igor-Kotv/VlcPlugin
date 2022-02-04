namespace Loupedeck.VlcPlugin
{
    using System;


    class VolumeAdjustment : PluginDynamicAdjustment
    {
        private Double _initialVolume = VlcPlugin.InitialVolume;
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public VolumeAdjustment() : base("Volume", "Volume adjustment", "Level", true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (ticks > 0)
            {
                if (this._initialVolume < 320)
                {
                    this._initialVolume += 2.56;
                }

                this._vlcPlugin.AdjustVolume((Int32)Math.Round(this._initialVolume));
            }
            else
            {
                if (this._initialVolume > 0)
                {
                    this._initialVolume -= 2.56;
                }

                this._vlcPlugin.AdjustVolume((Int32)Math.Round(this._initialVolume));
            }
            this.ActionImageChanged(actionParameter);

        }

        protected override void RunCommand(String actionParameter)
        {
            this._initialVolume = 0;
            this._vlcPlugin.AdjustVolume((Int32)this._initialVolume);
            this.ActionImageChanged(actionParameter);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._initialVolume < 30
                ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.VolumeLow.png")
                : this._initialVolume > 30 && this._initialVolume < 150
                ? EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.VolumeMedium.png")
                : EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.VolumeHigh.png");

        }
        protected override String GetAdjustmentValue(String actionParameter)
        {
            var displayVolume = (Int32)Math.Round(this._initialVolume / 256 * 100);
            return displayVolume.ToString();
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) => "Reset Volume";
    }

}
