namespace Loupedeck.Vlc.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    internal class PlaylistTrackPlayCommandFolder : PluginDynamicFolder
    {
        private readonly Vlc _vlcPlugin = new Vlc();

        public PlaylistTrackPlayCommandFolder()
        {
            this.DisplayName = "Playlist";
            this.GroupName = "Playback";
            this.Description = "Play any track from your playlist";
            this.Navigation = PluginDynamicFolderNavigation.EncoderArea;
        }

        public override BitmapImage GetButtonImage(PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.TogglePlay.png");

        public override IEnumerable<String> GetButtonPressActionNames()
        {
            var playlist = Vlc.GetPlaylistInfo();
            return playlist != null && playlist.Any() ? playlist.Select(x => this.CreateCommandName(x.Id)) : new List<String>();
        }

        public override String GetCommandDisplayName(String commandParameter, PluginImageSize imageSize)
        {
            var playlist = Vlc.GetPlaylistInfo();
            var track = playlist?.FirstOrDefault(x => x.Id == commandParameter);
            if (null != track)
            {
                var trackDisplayName = track.Current.IsNullOrEmpty() ? track?.Name : $"Playing:\n{track?.Name}";

                return trackDisplayName;
            }
            return "";
        }

        public override BitmapImage GetCommandImage(String commandParameter, PluginImageSize imageSize) => this.TrackIsCurrent(commandParameter)
            ? EmbeddedResources.ReadImage("Loupedeck.Vlc.Resources.ActionImages.Width90.PlayPause.png")
            : null;

        public override void RunCommand(String commandParameter)
        {
            var playlist = Vlc.GetPlaylistInfo();
            var track = playlist.FirstOrDefault(x => x.Id == commandParameter);
            try
            {
                if (track.Current.IsNullOrEmpty())
                {
                    this._vlcPlugin.PlayTrack(commandParameter);
                }
                else
                { this._vlcPlugin.Play(); }

                this.CommandImageChanged(commandParameter);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Playlist is empty or action obtain an error: ", e);
            }
        }

        private Boolean TrackIsCurrent(String commandParameter)
        {
            var playlist = Vlc.GetPlaylistInfo();
            var track = playlist?.FirstOrDefault(x => x.Id == commandParameter);
            return !track.Current.IsNullOrEmpty();
        }
    }

}
