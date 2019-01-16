using System;
using System.IO;
using System.Threading.Tasks;
using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Gui;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;
using VS4Mac.SkiaSharpFiddle.Controllers;
using VS4Mac.SkiaSharpFiddle.Controllers.Base;
using VS4Mac.SkiaSharpFiddle.Views.Base;

namespace VS4Mac.SkiaSharpFiddle.Views
{
    public interface ISkiaSharpFiddleView : IView
    {

    }

    public class SkiaSharpFiddleView : AbstractXwtViewContent, ISkiaSharpFiddleView
    {
        static readonly SKColor PaneColor = 0xFFF5F5F5;
        static readonly SKColor AlternatePaneColor = 0xFFF0F0F0;
       
        const int BaseBlockSize = 8;

        XwtControl _control;
        VBox _mainBox;
        HBox _headerBox;
        Label _widthLabel;
        Entry _widthEntry;
        Label _heightLabel;
        Entry _heightEntry;
        HPaned _contentBox;
        Frame _codeFrame;
        VBox _codeBox;
        Label _codeLabel;
        TextEditor _textEditor;
        Frame _outputFrame;
        VBox _outputBox;
        Label _outputLabel;
        SKWidget _skWidget;
        Frame _messagesFrame;
        VBox _messagesBox;
        Label _messagesLabel;
        TreeView _messagesView;
        ListStore _messagesStore;

        SkiaSharpFiddleController _controller;

        public SkiaSharpFiddleView()
        {
            Init();
            BuildGui();
            AttachEvents();
        }

        public override Xwt.Widget Widget => _control;

        public override bool IsViewOnly
        {
            get
            {
                return true;
            }
        }

        public override bool IsFile
        {
            get
            {
                return false;
            }
        }

        void Init()
        {
            _mainBox = new VBox();
            _headerBox = new HBox();

            _widthLabel = new Label("Width:");
            _widthLabel.SetPadding(6, 0);

            _widthEntry = new Entry
            {
                WidthRequest = 60
            };

            _heightLabel = new Label("Height:");
            _heightLabel.SetPadding(6, 0);

            _heightEntry = new Entry
            {
                WidthRequest = 60
            };

            _contentBox = new HPaned();

            _codeFrame = new Frame
            {
                BorderWidth = 1
            };

            _codeBox = new VBox();
            _codeLabel = new Label("Code");
            _textEditor = TextEditorFactory.CreateNewEditor();
            _textEditor.MimeType = "text/x-csharp";

            _outputFrame = new Frame
            {
                BorderWidth = 1,
                WidthRequest = 512
            };

            _outputBox = new VBox();
            _outputLabel = new Label("Output");
            _skWidget = new SKWidget();

            _messagesFrame = new Frame
            {
                BorderWidth = 1
            };

            _messagesBox = new VBox();
            _messagesLabel = new Label("Messages");

            _messagesView = new TreeView
            {
                HeadersVisible = true,
                HeightRequest = 60,
                WidthRequest = 400
            };

            _messagesView.RulesHint = true;
            _messagesView.Selection.Mode = Gtk.SelectionMode.Single;

            _messagesView.AppendColumn("Line", new CellRendererText(), "text", 0);
            _messagesView.AppendColumn("Message", new CellRendererText(), "text", 1);

            _messagesStore = new ListStore(typeof(string), typeof(string));

            _messagesView.Model = _messagesStore;

            var xwtMainBox = Xwt.Toolkit.CurrentEngine.WrapWidget(_mainBox);
            _control = new XwtControl(xwtMainBox);
        }

        void BuildGui()
        {
            ContentName = "SkiaSharp Fiddle";

            _headerBox.PackStart(_widthLabel, false, false, 0);
            _headerBox.PackStart(_widthEntry, false, false, 0);
            _headerBox.PackStart(_heightLabel, false, false, 0);
            _headerBox.PackStart(_heightEntry, false, false, 0);

            _codeBox.PackStart(_codeLabel, false, false, 0);
            _codeBox.PackEnd(_textEditor, true, true, 0);
            _codeFrame.Add(_codeBox);

            _outputBox.PackStart(_outputLabel, false, false, 0);
            _outputBox.PackEnd(_skWidget, true, true, 0);
            _outputFrame.Add(_outputBox);

            _contentBox.Pack1(_codeFrame, true, true);
            _contentBox.Pack2(_outputFrame, false, false);

            _messagesBox.PackStart(_messagesLabel, false, false, 0);
            _messagesBox.PackEnd(_messagesView, true, true, 0);
            _messagesFrame.Add(_messagesBox);

            _mainBox.PackStart(_headerBox, false, false, 0);
            _mainBox.PackStart(_contentBox, true, true, 0);
            _mainBox.PackStart(_messagesFrame, true, true, 0);

            _mainBox.ShowAll();
        }

        void AttachEvents()
        {
            _widthEntry.Changed += OnWidthEntryChanged;
            _heightEntry.Changed += OnHeightEntryChanged;
            _textEditor.TextChanged += OnEditorTextChanged;
            _skWidget.PaintSurface += OnPaintSurface;
        }

        public void SetController(IController controller)
        {
            _controller = (SkiaSharpFiddleController)controller;

            LoadDefaultValues();
            LoadInitialSourceAsync().GetAwaiter().GetResult();
        }

        internal void LoadDefaultValues()
        {
            _widthEntry.Text = _controller.DrawingWidth.ToString();
            _heightEntry.Text = _controller.DrawingHeight.ToString();
        }

        internal async Task LoadInitialSourceAsync()
        {
            var type = typeof(SkiaSharpFiddleView);
            var assembly = type.Assembly;

            var resource = "VS4Mac.SkiaSharpFiddle.Resources.InitialSource.cs";

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                using (var reader = new StreamReader(stream))
                {
                    _textEditor.Text = await reader.ReadToEndAsync();
                }
            }

            _controller.Compile(_textEditor.Text);
            LoadErrors();
            _skWidget.QueueDraw();
        }

        internal void LoadErrors()
        {
            _messagesStore.Clear();

            foreach (var compilationMessage in _controller.CompilationMessages)
            {
                _messagesStore.AppendValues(compilationMessage.LineNumber.ToString(), compilationMessage.Message);
            }

            _mainBox.ShowAll();
        }

        void OnWidthEntryChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(_widthEntry.Text, "[^0-9]"))
            {
                _widthEntry.Text = _widthEntry.Text.Remove(_widthEntry.Text.Length - 1);
            }

            _controller.DrawingWidth = Convert.ToInt32(_widthEntry.Text);
            _skWidget.QueueDraw();
        }

        void OnHeightEntryChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(_heightEntry.Text, "[^0-9]"))
            {
                _heightEntry.Text = _heightEntry.Text.Remove(_heightEntry.Text.Length - 1);
            }

            _controller.DrawingHeight = Convert.ToInt32(_heightEntry.Text);
            _skWidget.QueueDraw();
        }

        void OnEditorTextChanged(object sender, MonoDevelop.Core.Text.TextChangeEventArgs e)
        {
            _controller.Compile(_textEditor.Text);
            LoadErrors();
            _skWidget.QueueDraw();
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var width = e.Info.Width;
            var height = e.Info.Height;

            var canvas = e.Surface.Canvas;

            canvas.Clear(PaneColor);

            canvas.ClipRect(SKRect.Create(_controller.DrawingSize));

            DrawTransparencyBackground(canvas, width, height, 1.0f);

            if (_controller.RasterDrawing != null)
                canvas.DrawImage(_controller.RasterDrawing, 0, 0);
        }

        void DrawTransparencyBackground(SKCanvas canvas, int width, int height, float scale)
        {
            var blockSize = BaseBlockSize * scale;

            var offsetMatrix = SKMatrix.MakeScale(2 * blockSize, blockSize);
            var skewMatrix = SKMatrix.MakeSkew(0.5f, 0);
            SKMatrix.PreConcat(ref offsetMatrix, ref skewMatrix);

            using (var path = new SKPath())
            {
                using (var paint = new SKPaint())
                {
                    path.AddRect(SKRect.Create(blockSize / -2, blockSize / -2, blockSize, blockSize));

                    paint.PathEffect = SKPathEffect.Create2DPath(offsetMatrix, path);
                    paint.Color = AlternatePaneColor;

                    canvas.DrawRect(SKRect.Create(width + blockSize, height + blockSize), paint);
                }
            }
        }
    }
}