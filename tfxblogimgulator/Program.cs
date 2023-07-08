using System.Text.RegularExpressions;
using ImageMagick;

var directory = args?[0];
var expression = args?[1];
var extension = args?[2];
var exportName = args?[3];
var nudgeAmount = args?[4];

var dir = new DirectoryInfo(directory);
var regex = new Regex(expression);
var filesProcessed = dir.GetFiles($"*.{extension}").Select(
    f => new
    {
        info = f,
        ord = Int32.Parse(regex.Match(f.Name).Captures[0].Value)
    })
    .OrderBy(f => f.ord)
    .ToList();

using var imageMagicCol = new MagickImageCollection();

// target 4:3
// import size assumed to be 1920x1080

var nudge = float.Parse(nudgeAmount ?? "0");
var crop = new MagickGeometry((int)((0.0625 + nudge) * 1920), 0, 
    percentageWidth: new Percentage(75), percentageHeight: new Percentage(100));
var resizeGeo = new MagickGeometry(0, 0, percentageWidth: new Percentage(40), percentageHeight: new Percentage(40));
var animDelay = (int)((1 / 60) * 100);

var ind = 0;
foreach (var image in filesProcessed)
{
    if (ind++ % 2 == 1) continue;
    var img = new MagickImage(image.info);
    img.Crop(crop);
    img.RePage();
    img.InterpolativeResize(resizeGeo, PixelInterpolateMethod.Nearest);
    //img.AnimationDelay = animDelay;
    img.AnimationDelay = 1;
    img.GifDisposeMethod = GifDisposeMethod.Previous;
    imageMagicCol.Add(img);
}

//imageMagicCol.Coalesce();

var settings = new QuantizeSettings
{
    Colors = 256,
};


imageMagicCol.Quantize(settings);
//imageMagicCol.Optimize();
imageMagicCol.Write(Directory.GetCurrentDirectory() + $"/{(exportName ?? "export")}.gif");