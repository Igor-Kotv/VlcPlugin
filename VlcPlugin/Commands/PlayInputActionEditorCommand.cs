namespace Loupedeck.VlcPlugin
{

    using System;

    class PlayInputActionEditorCommand : ActionEditorCommand
    {
        private readonly VlcPlugin _vlcPlugin = new ();

        private const String FilePathOrUrlControlName = "FilePath";

        public PlayInputActionEditorCommand()
        {
            this.DisplayName = "Play Media";

            this.Description = "Starts playing specified media file or folder with media files";

            this.ActionEditor.AddControlEx(
                new ActionEditorFileSelector(name: FilePathOrUrlControlName, labelText: "File or URL to a media file:")
                );
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var value = actionParameters.GetString(FilePathOrUrlControlName);

            if (value.IsNullOrEmpty())
            {
                return false;
            }

            if (!value.IsNullOrEmpty())
            {
                this._vlcPlugin.InputPlay(value);
            }

            return true;
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.PlayMedia.png");
    }
}