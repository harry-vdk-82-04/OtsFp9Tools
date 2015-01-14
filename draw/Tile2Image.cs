using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Net;
using System.Text;

namespace Ots.draw
{
    using fp9;

    public class Tile2Image
    {

        public PointF GetTopPoint(Map.Location loc, SizeF center)
        {
            var point = GetCenterPoint(loc, center);
            point.Y -= center.Height;
            return point;
        }

        public PointF GetCenterPoint(Map.Location loc, SizeF center)
        {
            var point = GetCenterPoint(loc.LocPos);
            point.X -= center.Width / 2;
            point.Y -= center.Height / 2;
            return point;
        }

        public PointF GetBottomPoint(Map.Location loc, SizeF center)
        {
            var point = GetCenterPoint(loc, center);
            point.Y += center.Height;
            return point;
        }

        public RectangleF GetRectangle(Map.Pos pos, Map.Pos size, Size maxSize)
        {
            var offset = GetCenterPoint(new Map.Pos(1, 1));
            var endPoint = GetCenterPoint(size);
            var rect = new RectangleF();
            rect.Location = PointF.Subtract(GetCenterPoint(pos), new SizeF(offset.X, offset.Y));
            var width = Math.Min(endPoint.X + offset.X, (float)maxSize.Width - rect.Location.X);
            var height = Math.Min(endPoint.Y + offset.Y, (float)maxSize.Height -rect.Location.Y);
            rect.Size = new SizeF(width, height);
            return rect;
        }

        public PointF GetCenterPoint(Map.Pos pos)
        {
            var colX = (float)(37.0 + (((float)(pos.Col - 1)) * 5542.0) / 100.0);
            var rowY = (float)(32.0 + (32.0 * ((float)((pos.Col - 1) % 2))) + (64.0 * ((float)(pos.Row - 1))));
            var point = new PointF(colX, rowY);
            return point;
        }
    }
}
