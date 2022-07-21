namespace Loupedeck.VlcPlugin
{
    using System;
    using System.IO;

    public class VlcApplication : ClientApplication
    {
        public VlcApplication()
        {

        }

        protected override String GetProcessName() => "vlc";

        protected override String GetBundleName() => "org.videolan.vlc";
    }
}