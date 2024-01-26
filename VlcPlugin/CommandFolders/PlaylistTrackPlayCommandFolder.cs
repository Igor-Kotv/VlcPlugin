namespace Loupedeck.VlcPlugin.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    internal class PlaylistTrackPlayCommandFolder : PluginDynamicFolder
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public PlaylistTrackPlayCommandFolder()
        {
            this.DisplayName = "Playlist";
            this.GroupName = "Playback";
            this.Description = "Play any track from your playlist";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType deviceType) =>
            deviceType == DeviceType.Loupedeck50 ||
            deviceType == DeviceType.Loupedeck60 ?
            PluginDynamicFolderNavigation.ButtonArea : PluginDynamicFolderNavigation.EncoderArea;

        public override BitmapImage GetButtonImage(PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.TogglePlay.png");

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType _)
        {
            var playlist = this._vlcPlugin.GetPlaylistInfo();
            return playlist != null && playlist.Any() ? playlist.Select(x => this.CreateCommandName(x.Id)) : new List<String>();
        }

        public override String GetCommandDisplayName(String commandParameter, PluginImageSize imageSize)
        {
            var playlist = this._vlcPlugin.GetPlaylistInfo();
            var track = playlist?.FirstOrDefault(x => x.Id == commandParameter);

            return null != track ? track.Current.IsNullOrEmpty() ? track?.Name : $"Playing:\n{track?.Name}" : "";
        }

        public override BitmapImage GetCommandImage(String commandParameter, PluginImageSize imageSize)
        {
            var trackImage = EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.PlayPause.png");

            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                if (this._vlcPlugin.TryGetCoverArt(imageSize, out var coverArt))
                {
                    bitmapBuilder.DrawImage(coverArt);
                }
                bitmapBuilder.DrawImage(trackImage);
                return this.TrackIsCurrent(commandParameter) ? bitmapBuilder.ToImage() : null;
            }
        }

        public override void RunCommand(String commandParameter)
        {
            var playlist = this._vlcPlugin.GetPlaylistInfo();
            var track = playlist.FirstOrDefault(x => x.Id == commandParameter);
            try
            {
                if (track.Current.IsNullOrEmpty())
                {
                    this._vlcPlugin.PlayTrack(commandParameter);
                }
                else
                { this._vlcPlugin.Play(); }

                this.ButtonActionNamesChanged();
            }
            catch (Exception e)
            {
                Tracer.Trace($"Playlist is empty or action obtain an error: ", e);
            }
        }

        private Boolean TrackIsCurrent(String commandParameter)
        {
            var playlist = this._vlcPlugin.GetPlaylistInfo();
            var track = playlist?.FirstOrDefault(x => x.Id == commandParameter);
            return !track.Current.IsNullOrEmpty();
        }
    }

}
