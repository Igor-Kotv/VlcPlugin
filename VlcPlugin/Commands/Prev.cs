namespace Loupedeck.VlcPlugin
{

    using System;

    class Prev : PluginDynamicCommand
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public Prev() : base("Previous", "Selects previous track", "Playback navigation")
        {
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Previous();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.PlaybackTrackPrevious.png");
    }

}