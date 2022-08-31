namespace Loupedeck.Vlc
{

    using System;

    class Prev : PluginDynamicCommand
    {
        private readonly Vlc _vlcPlugin = new Vlc();

        public Prev() : base("Previous", "Selects previous track", "Playback navigation")
        {
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Previous();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.PlaybackTrackPrevious.png");
    }

}