namespace Loupedeck.Vlc
{

    using System;

    class Next : PluginDynamicCommand
    {
        private readonly Vlc _vlcPlugin = new Vlc();

        public Next() : base("Next", "Selects next track", "Playback navigation")
        {
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Next();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.PlaybackTrackNext.png");
    }

}