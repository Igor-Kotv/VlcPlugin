namespace Loupedeck.VlcPlugin
{
    using System;


    class NavigationAdjustment : PluginDynamicAdjustment
    {

        public NavigationAdjustment() : base("Play and Navigate", "Selects track in playlist", "Playback", true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (ticks > 0)
            {
                VlcPlugin.Next();
            }
            else
            {
                VlcPlugin.Previous();
            }
        }

        protected override void RunCommand(String actionParameter) => VlcPlugin.Play();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width50.PlayAndNavigateTracks.png");

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) => "Play/Pause";
    }

}
