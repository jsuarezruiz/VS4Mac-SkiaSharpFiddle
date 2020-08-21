using System;
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using VS4Mac.SkiaSharpFiddle.Controllers;
using VS4Mac.SkiaSharpFiddle.Views;

namespace VS4Mac.SkiaSharpFiddle.Commands
{
    public class SkiaSharpFiddleCommand : CommandHandler
    {
        const string PadId = "SkiaSharpFiddler.Pad";

        protected override void Run()
        {
            var pad = GetActivePad();

            if (pad == null)
            {
                var skiaSharpFiddleView = new SkiaSharpFiddleView();
                var skiaSharpFiddleController = new SkiaSharpFiddleController(skiaSharpFiddleView);

                pad = IdeApp.Workbench.ShowPad(skiaSharpFiddleView, PadId, "SkiaSharp Fiddler", "Center", null);

                if (pad == null)
                {
                    return;
                }
            }

            pad.Sticky = true;
            pad.AutoHide = false;
            pad.BringToFront();
        }

        public static bool IsPadOpen()
        {
            return GetActivePad() != null;
        }

        public static Pad GetActivePad()
        {
            var pads = IdeApp.Workbench.Pads;

            foreach (var pad in pads)
            {
                if (string.Equals(pad.Id, PadId, StringComparison.OrdinalIgnoreCase))
                {
                    return pad;
                }
            }

            return null;
        }
    }
}