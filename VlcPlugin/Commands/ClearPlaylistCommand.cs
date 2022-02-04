namespace Loupedeck.VlcPlugin
{

    using System;

    class ClearPlaylistCommand : PluginDynamicCommand
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public ClearPlaylistCommand() : base("Clear Playlist", "Removes all media from playlist!", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.Empty();
    }

}