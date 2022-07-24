namespace Loupedeck.Vlc
{
    using System;

    public class VlcApplication : ClientApplication
    {
        public VlcApplication()
        {

        }

        protected override String GetProcessName() => "vlc";

        protected override String GetBundleName() => "org.videolan.vlc";
    }
}