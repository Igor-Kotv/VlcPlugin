namespace Loupedeck.VlcPlugin.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    internal class PlaylistTrackPlayCommandFolder : PluginDynamicFolder
    {
        public PlaylistTrackPlayCommandFolder()
        {
            this.DisplayName = "Playlist";
            this.GroupName = "Playback";
            this.Description = "Play any track from your playlist";
            this.Navigation = PluginDynamicFolderNavigation.EncoderArea;
        }

        public override BitmapImage GetButtonImage(PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.TogglePlay.png");
            return bitmapImage;
        }

        public override IEnumerable<String> GetButtonPressActionNames()
        {
            var playlist = VlcPlugin.GetPlaylistInfo();
            return playlist != null && playlist.Any() ? playlist.Select(x => this.CreateCommandName(x.Id)) : new List<String>();
        }

        public override String GetCommandDisplayName(String commandParameter, PluginImageSize imageSize)
        {
            var playlist = VlcPlugin.GetPlaylistInfo();
            var track = playlist?.FirstOrDefault(x => x.Id == commandParameter);
            if (null != track)
            {
                var trackDisplayName = track.Current.IsNullOrEmpty() ? track?.Name : $"Playing:\n{track?.Name}";

                return trackDisplayName;
            }
            return "";
        }

        public override void RunCommand(String commandParameter)
        {
            var playlist = VlcPlugin.GetPlaylistInfo();
            var track = playlist.FirstOrDefault(x => x.Id == commandParameter);
            try
            {
                if (track.Current.IsNullOrEmpty())
                {
                    VlcPlugin.PlayTrack(commandParameter);
                }
                else
                { VlcPlugin.Play(); }

                this.CommandImageChanged(commandParameter);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Playlist is empty or action obtain an error: ", e);
            }
        }
    }

}
