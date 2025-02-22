using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml;

namespace PMEditor
{
    /// <summary>
    /// CodeViewer.xaml 的交互逻辑
    /// </summary>
    public partial class CodeViewer : Page
    {

        public CodeViewer()
        {
            IHighlightingDefinition highlightingDefinition;
            string projName = Assembly.GetExecutingAssembly().GetName().Name!;
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(projName + ".Json.xshd")!)
            {
                using XmlReader reader = new XmlTextReader(s);
                highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            HighlightingManager.Instance.RegisterHighlighting("JsonHighlighting", new[] { ".json" }, highlightingDefinition);
            InitializeComponent();
            jsonViewer.Text = EditorWindow.Instance.track.ToJsonString();
            jsonViewer.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JsonHighlighting");
        }
    }
}
