using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SkiaSharp;
using VS4Mac.SkiaSharpFiddle.Controllers.Base;
using VS4Mac.SkiaSharpFiddle.Models;
using VS4Mac.SkiaSharpFiddle.Services;
using VS4Mac.SkiaSharpFiddle.Views;

namespace VS4Mac.SkiaSharpFiddle.Controllers
{
    public class SkiaSharpFiddleController : IController
    {
        readonly int DefaultDrawingWidth = 512;
        readonly int DefaultDrawingHeight = 512;
       
        int _drawingWidth;
        int _drawingHeight;

        readonly ISkiaSharpFiddleView _view;
        readonly CompilerService _compiler;

        public SkiaSharpFiddleController(ISkiaSharpFiddleView view)
        {
            _view = view;
            _compiler = new CompilerService();

            DrawingWidth = DefaultDrawingWidth;
            DrawingHeight = DefaultDrawingHeight;
            CompilationMessages = new List<CompilationMessage>();

            view.SetController(this);
        }

        public int DrawingWidth 
        { 
            get { return _drawingWidth; }
            set 
            {
                _drawingWidth = value;
                GenerateDrawings();
            } 
        }

        public int DrawingHeight
        {
            get { return _drawingHeight; }
            set
            {
                _drawingHeight = value;
                GenerateDrawings();
            }
        }

        public SKSizeI DrawingSize => new SKSizeI(DrawingWidth, DrawingHeight);

        public SKImageInfo ImageInfo => new SKImageInfo(DrawingWidth, DrawingHeight);

        public List<CompilationMessage> CompilationMessages { get; }

        public SKImage RasterDrawing { get; set; }

        public SKImage GpuDrawing { get; set; }

        public void Compile(string sourceCode)
        {
            CompilationMessages.Clear();

            var diagnostics = _compiler.Compile(sourceCode);

            var messages = GetCompilationMessages(diagnostics);

            foreach(var message in messages)
                CompilationMessages.Add(message);

            GenerateDrawings();
        }

        internal IEnumerable<CompilationMessage> GetCompilationMessages(IEnumerable<Diagnostic> diagnostics)
        {
            diagnostics = diagnostics
                .Where(d => d.Location.IsInSource)
                .Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning)
                .OrderBy(d => d.Severity)
                .OrderBy(d => d.Location.SourceSpan.Start);

            foreach (var diag in diagnostics)
            {
                yield return new CompilationMessage
                {
                    IsError = diag.Severity == DiagnosticSeverity.Error,
                    Message = $"{diag.Severity.ToString().ToLowerInvariant()} {diag.Id}: {diag.GetMessage()}",
                    StartOffset = diag.Location.SourceSpan.Start,
                    EndOffset = diag.Location.SourceSpan.End,
                    LineNumber = diag.Location.GetMappedLineSpan().Span.Start.Line + 1
                };
            }
        }

        internal void GenerateDrawings()
        {
            var old = RasterDrawing;

            var info = ImageInfo;
            using (var surface = SKSurface.Create(info))
            {
                _compiler.Draw(surface, info.Size);
                RasterDrawing = surface?.Snapshot();
            }

            old?.Dispose();
        }
    }
}