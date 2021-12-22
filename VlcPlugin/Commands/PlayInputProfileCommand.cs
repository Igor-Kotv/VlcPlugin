namespace Loupedeck.VlcPlugin
{

    using System;

    class PlayInputProfileCommand : PluginDynamicCommand
    {
        public PlayInputProfileCommand() : base("Play Media", "Starts playing specified media file or folder with media files", "Play Media") => this.MakeProfileAction("text;Enter file path or URL to a madia file:");

        protected override void RunCommand(String actionParameter) => VlcPlugin.InputPlay(actionParameter);
    }
}