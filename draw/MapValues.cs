using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Ots.draw
{
    using fp9;
    public class MapValues : Tile2Image
    {
        private Font _font = new Font("Arial", 8, FontStyle.Regular);

        public bool DrawHexnumbers(Graphics graphics, Map map)
        {
            var pos = new Map.Pos();
            for (pos.Col = map.MapLocations.Min.Col; pos.Col <= map.MapLocations.Max.Col; pos.Col++)
            {
                for (pos.Row = map.MapLocations.Min.Row; pos.Row <= map.MapLocations.Max.Row; pos.Row++)
                {
                    DrawHexnumber(graphics, map.MapLocations.Square[pos.Col][pos.Row]);
                }
            }
            return true;
        }

        public bool DrawMapValues(Graphics graphics, Map map)
        {
            var pos = new Map.Pos();
            for (pos.Col = map.MapLocations.Min.Col; pos.Col <= map.MapLocations.Max.Col; pos.Col++)
            {
                for (pos.Row = map.MapLocations.Min.Row; pos.Row <= map.MapLocations.Max.Row; pos.Row++)
                {
                    DrawMapValues(graphics, map.MapLocations.Square[pos.Col][pos.Row]);
                }
            }
            return true;
        }

        private void DrawHexnumber(Graphics graphics, Map.Location loc)
        {
            var text = string.Format("{0}.{1}", loc.LocPos.Col.ToString("D2"), loc.LocPos.Row.ToString("D2"));
            DrawTopString(graphics, loc, text);
        }

        private void DrawMapValues(Graphics graphics, Map.Location loc)
        {
            var text = string.Format("e{0},v{1},m{2}", loc.Elevation, loc.CoverRating, loc.MobilityRating);
            DrawCenterString(graphics, loc, text);
            DrawBottomString(graphics, loc, loc.RoadCode);
        }

        private void DrawTopString(Graphics graphics, Map.Location loc, string text)
        {
            var size = graphics.MeasureString(text, _font); // Get size of rotated text (bounding box)
            var point = GetTopPoint(loc, size);
            graphics.DrawString(text, _font, Brushes.Black, point.X + 1, point.Y + 1);
            graphics.DrawString(text, _font, Brushes.White, point.X, point.Y);
        }

        private void DrawCenterString(Graphics graphics, Map.Location loc, string text)
        {
            var size = graphics.MeasureString(text, _font); // Get size of rotated text (bounding box)
            var point = GetCenterPoint(loc, size);
            graphics.DrawString(text, _font, Brushes.Black, point.X + 1, point.Y + 1);
            graphics.DrawString(text, _font, Brushes.White, point.X, point.Y);
        }

        private void DrawBottomString(Graphics graphics, Map.Location loc, string text)
        {
            var size = graphics.MeasureString(text, _font); // Get size of rotated text (bounding box)
            var point = GetBottomPoint(loc, size);
            graphics.DrawString(text, _font, Brushes.Black, point.X + 1, point.Y + 1);
            graphics.DrawString(text, _font, Brushes.White, point.X, point.Y);
        }

    }
}
