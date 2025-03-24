using SeedHealthApp.Models;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SeedHealthApp.Custom.Controls
{
    public class SKRenderView : SKCanvasView
    {
        private Dictionary<string, SKPaint> PaintPalette = new Dictionary<string, SKPaint>()
        {
            { "positive", new SKPaint{ Style = SKPaintStyle.Fill, Color = SKColor.Parse("ffc4d6") } },
            { "negative", new SKPaint{ Style = SKPaintStyle.Fill, Color = SKColor.Parse("d0f4de") } },
            { "buffer", new SKPaint{ Style = SKPaintStyle.Fill, Color = SKColor.Parse("adb5bd") } },
            { "cccccc", new SKPaint{ Style = SKPaintStyle.Fill, Color = SKColor.Parse("cccccc") } },
            { "default", new SKPaint{ Style = SKPaintStyle.Stroke, Color = SKColor.Parse("cccccc") } }
        };
        public SKRenderView()
        {
            foreach (var color in ColorPalette.Colors)
            {
                PaintPalette.Add(color, new SKPaint { Style = SKPaintStyle.Fill, Color = SKColor.Parse(color) });
            }
        }
        public static readonly BindableProperty PlateCellsProperty = BindableProperty.Create(
            nameof(PlateCells),
            typeof(IEnumerable<PlateCell>),
            typeof(SKRenderView),
            Enumerable.Empty<PlateCell>(),
            BindingMode.OneWay,
            propertyChanged: OnPlateCellsChanged);
        public IEnumerable<PlateCell> PlateCells
        {
            get => (IEnumerable<PlateCell>)GetValue(PlateCellsProperty);
            set => SetValue(PlateCellsProperty, value);
        }
        private static void OnPlateCellsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((SKRenderView)bindable).InvalidateSurface();
        }
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            //new CircleRenderer().PaintSurface(e.Surface, e.Info);
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();

            var cellSpacing = 2;
            var cellWidth = (e.Info.Width - 13 * cellSpacing) / 12;
            var cellHeight = cellWidth;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    var position = (8 * x) + y + 1;

                    var found = PlateCells.FirstOrDefault(c => c.Position == position);
                    if (found != null)
                    {
                        var cellPaint = PaintPalette.ContainsKey(found.Color) ? PaintPalette[found.Color] : PaintPalette["cccccc"];
                        canvas.DrawRoundRect(cellSpacing + x * (cellWidth + cellSpacing),
                            cellSpacing + y * (cellHeight + cellSpacing), cellWidth, cellWidth, .25f, .25f, cellPaint);
                    }

                    canvas.DrawRoundRect(cellSpacing + x * (cellWidth + cellSpacing),
                        cellSpacing + y * (cellHeight + cellSpacing), cellWidth, cellWidth, .25f, .25f, PaintPalette["default"]);

                    //canvas.DrawText(position.ToString(),
                    //    cellSpacing + x * (cellWidth + cellSpacing),
                    //    cellSpacing + y * (cellHeight + cellSpacing) + cellHeight - cellSpacing,
                    //    PaintPalette["cccccc"]);
                    //    PositionTitle = Utils.ConvertPositionToPositionTitle(y, x),
                }
            }
        }
    }
}
