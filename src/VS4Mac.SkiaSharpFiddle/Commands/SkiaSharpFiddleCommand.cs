using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using VS4Mac.SkiaSharpFiddle.Controllers;
using VS4Mac.SkiaSharpFiddle.Views;

namespace VS4Mac.SkiaSharpFiddle.Commands
{
    public class SkiaSharpFiddleCommand : CommandHandler
    {
        protected override void Run()
        {
            var skiaSharpFiddleView = new SkiaSharpFiddleView();
            var skiaSharpFiddleController = new SkiaSharpFiddleController(skiaSharpFiddleView);
            IdeApp.Workbench.OpenDocument(skiaSharpFiddleView, true);
        }
    }
}