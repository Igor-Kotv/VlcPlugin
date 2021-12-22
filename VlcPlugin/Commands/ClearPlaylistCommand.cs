namespace Loupedeck.VlcPlugin
{

    using System;

    class ClearPlaylistCommand : PluginDynamicCommand
    {
        public ClearPlaylistCommand() : base("Clear Playlist", "Removes all media from playlist!", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter) => VlcPlugin.Empty();
    }

}