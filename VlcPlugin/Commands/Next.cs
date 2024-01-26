namespace Loupedeck.VlcPlugin
{

    using System;

    class Next : PluginDynamicCommand
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public Next() : base("Next", "Selects next track", "Playback navigation")
        {
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Next();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.PlaybackTrackNext.png");
    }

}