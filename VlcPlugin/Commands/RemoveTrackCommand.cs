namespace Loupedeck.Vlc
{

    using System;
    using System.Linq;

    class RemoveTrackCommand : PluginDynamicCommand
    {
        private readonly Vlc _vlcPlugin = new Vlc();

        public RemoveTrackCommand() : base("Remove Track", "Removes track from playlist", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            var playlist = this._vlcPlugin.GetPlaylistInfo();
            var trackInfo = this._vlcPlugin.GetTrackInfo();
            var itemIndex = playlist.IndexOf(item => item.Name == trackInfo.Category.Meta.Title);

            if (itemIndex <= playlist.Count && itemIndex >= 0)
            {
                var id = playlist.ElementAt(itemIndex).Id;
                this._vlcPlugin.DeleteTrack(id);
            }
        }
    }
}