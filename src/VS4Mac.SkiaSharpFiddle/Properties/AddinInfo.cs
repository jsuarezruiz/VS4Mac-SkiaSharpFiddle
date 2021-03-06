﻿using System;
using System.Runtime.InteropServices;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "SkiaSharpFiddle",
    Namespace = "MonoDevelop",
    Version = "0.2",
    Category = "IDE extensions"
)]

[assembly: AddinName("SkiaSharp Fiddle")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("SkiaSharp Fiddle is a SkiaSharp playground.")]
[assembly: AddinAuthor("Javier Suárez Ruiz")]
[assembly: AddinUrl("https://github.com/jsuarezruiz/VSMac-SkiaSharpFiddle")]

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]