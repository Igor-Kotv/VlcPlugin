namespace Loupedeck.VlcPlugin
{

    using System;

    class OpenFileCommand : PluginDynamicCommand
    {

        public OpenFileCommand() : base("Open File", "Opens file dialog", "Media")
        {
        }

        protected override void RunCommand(String actionParameter) => this.Plugin.ClientApplication.SendKeyboardShortcut(VirtualKeyCode.KeyO, ModifierKey.Control);

        //protected override Boolean OnLoad()
        //{
        //    this.Plugin.ServiceEvents.UrlCallbackReceived += this.OnUrlCallbackReceived;
        //    return true;
        //}

        //protected override Boolean OnUnload()
        //{
        //    this.Plugin.ServiceEvents.UrlCallbackReceived -= this.OnUrlCallbackReceived;
        //    return true;
        //}

        //private void OnUrlCallbackReceived(Object sender, UrlCallbackReceivedEventArgs e)
        //{
        //    // loupedeck:callback/plugin/Vlc/loupedeck
        //    if (!e.Parameters.IsNullOrEmpty())
        //    {
        //        VlcPlugin.Password = e.Parameters;
        //        VlcPlugin.ConnectVlc();
        //    }
        //}

    }

}

