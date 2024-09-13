using PMEditor.Controls;
using PMEditor.Util;
using System.IO;

namespace PMEditor
{
    public partial class Function
    {
        public FunctionRectangle rectangle;

        public Line parentLine;

        public FileInfo linkedFile;

        public string LegalName => DatapackGenerator.ToLegalIdentifier(functionName);

        public void Rename(string newName)
        {
            if (!linkedFile.Exists)
            {
                linkedFile.Create().Close();
            }
            string newPath = Path.Combine(linkedFile.DirectoryName!, newName + ".mcfunction");
            File.Move(linkedFile.FullName, newPath);
            linkedFile = new FileInfo(newPath);
            this.functionName = newName;
        }

        public bool TryLink(Track track, bool autoCreate = false)
        {
            var dir = track.datapack.FrameFunction;
            linkedFile = new FileInfo(Path.Combine(dir.FullName, LegalName + ".mcfunction"));
            if(linkedFile.Exists)
            {
                return true;
            }
            else
            {
                if(autoCreate)
                {
                    linkedFile.Create().Close();
                    return true;
                }
                return false;
            }
        }
    }
}
