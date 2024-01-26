namespace Loupedeck.VlcPlugin
{

    using System;

    class PlayInputProfileCommand : PluginDynamicCommand
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        public PlayInputProfileCommand() : base("Play Media (legacy)", "Deprecated template to preserve previously created actions. Please use a new Play media action template to create the action.", "Play Media") => this.MakeProfileAction("text;Enter file path or URL to a media file:");

        protected override void RunCommand(String actionParameter) => this._vlcPlugin.InputPlay(actionParameter);
    }
}