namespace Loupedeck.Vlc
{

    using System;

    class PlayInputProfileCommand : PluginDynamicCommand
    {
        private readonly Vlc _vlcPlugin = new Vlc();

        public PlayInputProfileCommand() : base("Play Media", "Starts playing specified media file or folder with media files", "Play Media") => this.MakeProfileAction("text;Enter file path or URL to a media file:");

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.InputPlay(actionParameter);
    }
}