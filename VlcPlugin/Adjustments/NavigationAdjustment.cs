namespace Loupedeck.VlcPlugin
{
    using System;


    class NavigationAdjustment : PluginDynamicAdjustment
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public NavigationAdjustment() : base("Navigate", "Selects track in playlist", "Playback navigation", false)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (ticks > 0)
            {
                this._vlcPlugin.Next();
            }
            else
            {
                this._vlcPlugin.Previous();
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.NavigateTracks.png");
    }

}
