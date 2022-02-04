namespace Loupedeck.VlcPlugin
{

    using System;

    class PlayInputProfileCommand : PluginDynamicCommand
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public PlayInputProfileCommand() : base("Play Media", "Starts playing specified media file or folder with media files", "Play Media") => this.MakeProfileAction("text;Enter file path or URL to a madia file:");

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.InputPlay(actionParameter);
    }
}