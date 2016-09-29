using System;
using System.Collections;
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

        public bool DrawHexNumbers(Graphics graphics, Map map)
        {
            var pos = new Map.Pos();
            for (pos.Col = map.MapLocations.Min.Col; pos.Col <= map.MapLocations.Max.Col; pos.Col++)
            {
                for (pos.Row = map.MapLocations.Min.Row; pos.Row <= map.MapLocations.Max.Row; pos.Row++)
                {
                    DrawHexNumber(graphics, map.MapLocations.Square[pos.Col][pos.Row]);
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

        public bool DrawElevation(Graphics graphics, Map map)
        {
            var pos = new Map.Pos();
            var colors = GetElevationColors().ToArray();
            foreach (var location in map.MapLocations.GetAllLocations()
                .OrderBy(loc => loc.Elevation))
            {
                var hexPoints = GetHexPoints(location.LocPos).ToArray();
                graphics.FillPolygon( new SolidBrush(colors[location.Elevation]), hexPoints);
                DrawHexNumber(graphics, location);
            }
            foreach (var location in map.MapLocations.GetAllLocations()
                .Where(loc => loc.Elevation >= 2))
            {
                DrawHexContours(graphics, map, location);
            }
            return true;
        }

        public bool DrawHexContours(Graphics graphics, Map map)
        {
            var pos = new Map.Pos();
            foreach (var location in map.MapLocations.GetAllLocations()
                .Where(loc => loc.Elevation >= 2))
            {
                DrawHexContours(graphics, map, location);
            }
            return true;
        }

        private void DrawHexContours(Graphics graphics, Map map, Map.Location location)
        {
            var locPos = location.LocPos;
            foreach (var neighbour in GetHexNeighbours(location.LocPos)
                .Where(x => map.MapLocations.WithinRange(x)))
            {
                var neighbourLocation = map.MapLocations.Square[neighbour.Col][neighbour.Row];
                if (neighbourLocation.Elevation >= location.Elevation) continue;
                graphics.DrawLine(new Pen(neighbour.Color,3), neighbour.Point1, neighbour.Point2);
            }
        }

        private void DrawHexNumber(Graphics graphics, Map.Location loc)
        {
            var text = string.Format("{0}.{1}", loc.LocPos.Col.ToString("D2"), loc.LocPos.Row.ToString("D2"));
            DrawTopString(graphics, loc, text);
        }

        private void DrawMapValues(Graphics graphics, Map.Location loc)
        {
            var text = string.Format("e{0},v{1},m{2}", loc.Elevation, loc.CoverRating, loc.MobilityRating);
            DrawCenterString(graphics, loc, text);
            int id;
            int.TryParse(loc.ObstacleCode, out id);
            text = string.Format("{0},{1}", 
                (id != 0 ? ""+ loc.ObstacleCode : "-"),
                (string.IsNullOrEmpty(loc.RoadCode) ? "-" : "R"));
            DrawBottomString(graphics, loc, text);
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

        private IEnumerable<Color> GetElevationColors()
        {
            yield return Color.Black;
            yield return Color.FromArgb(164,175,106); // Level 1
            yield return Color.FromArgb(132, 142, 81); // Level 2
            yield return Color.FromArgb(109, 117, 64); // Level 3
            yield return Color.FromArgb(86, 94, 46); // Level 4
            yield return Color.FromArgb(67, 74, 31); // Level 5

            yield return Color.FromArgb(85, 76, 49); // Level 6
            yield return Color.FromArgb(109, 99, 68); // Level 7
            yield return Color.FromArgb(137, 125, 90); // Level 8
            yield return Color.FromArgb(168, 154, 114); // Level 9
            yield return Color.FromArgb(203, 189, 144); // Level 10
        }
    }
}
