namespace Loupedeck.VlcPlugin
{

    using System;

    class PlayInputActionEditorCommand : ActionEditorCommand
    {
        private readonly VlcPlugin _vlcPlugin = new VlcPlugin();

        private const String UrlControlName = "Url";

        private const String FilePathControlName = "FilePath";

        public PlayInputActionEditorCommand()
        {
            this.DisplayName = "Play Media";

            this.Description = "Starts playing specified media file or folder with media files";

            this.ActionEditor.AddControlEx(
                new ActionEditorFileSelector(name: FilePathControlName, labelText: "Select file to play:")
                );

            this.ActionEditor.AddControlEx(
                new ActionEditorTextbox(name: UrlControlName, labelText: "Enter URL to a media file:"));
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var url = actionParameters.GetString(UrlControlName);
            var filePath = actionParameters.GetString(FilePathControlName);

            if (url.IsNullOrEmpty() && filePath.IsNullOrEmpty())
            {
                return false;
            }

            if (!filePath.IsNullOrEmpty())
            {
                this._vlcPlugin.InputPlay(filePath);
            }

            if (!url.IsNullOrEmpty())
            {
                this._vlcPlugin.InputPlay(url);
            }

            return true;
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight) => EmbeddedResources.ReadImage("Loupedeck.VlcPlugin.Resources.ActionImages.Width90.PlayMedia.png");
    }
}