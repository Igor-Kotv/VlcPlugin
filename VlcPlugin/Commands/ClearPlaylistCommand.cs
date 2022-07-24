namespace Loupedeck.Vlc
{

    using System;

    class ClearPlaylistCommand : PluginDynamicCommand
    {
        private readonly Vlc _vlcPlugin = new Vlc();

        public ClearPlaylistCommand() : base("Clear Playlist", "Removes all media from playlist!", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Empty();
    }

}