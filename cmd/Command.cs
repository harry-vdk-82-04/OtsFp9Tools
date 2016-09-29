using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Ots.cmd
{
    using fp9;
    using draw;

    public class Command
    {

        public class ExtractMap
        {
            public String Filename { get; set; }
            public Map.Pos StartPos { get; set; }
            public Map.Pos MapSize { get; set; }
            public bool IsDrawMapValues { get; set; }
            public bool HasFilename { get { return string.IsNullOrEmpty(Filename) == false; } }

            public ExtractMap()
            {
                Filename = string.Empty;
                StartPos = new Map.Pos();
                MapSize = new Map.Pos() { Col = 46, Row = 30 };
            }

            public ExtractMap(ExtractMap source)
            {
                Filename = source.Filename;
                StartPos = new Map.Pos(source.StartPos);
                MapSize = new Map.Pos(source.MapSize);
            }

            public Map.Pos GetOffset()
            {
                var offset = new Map.Pos()
                {
                    Col = StartPos.Col-1,
                    Row = StartPos.Row-1
                };
                return offset;
            }

            public override string ToString()
            {
                return string.Format("Filename={0}, Start={1}, MapSize={2}", Filename, StartPos, MapSize);
            }
        }
        public bool IsImport { get; set; }
        public bool IsDrawMapValues { get; set; }
        public bool IsCreateHexNumbers { get; set; }
        public bool IsCreateHexContours { get; set; }
        public bool IsCreateElevation { get; set; }
        public Map.Pos OffsetPos { get; set; }
        public Map.Range FilterRange { get; set; }
        public Map.Pos NewMax { get; set; }
        public String Filename { get; set; }
        public String ImportFile { get; set; }
        public String DrawFilename { get; set; }
        public String DrawExtension { get; set; }

        private readonly List<ExtractMap> _extractMaps = new List<ExtractMap>();
        public List<ExtractMap> ExtractMaps {get { return _extractMaps; } }

        public Command()
        {
            IsImport = false;
            IsDrawMapValues = false;
            OffsetPos = new Map.Pos();
            NewMax = new Map.Pos();
            FilterRange = new Map.Range();
            Filename = string.Empty;
            ImportFile = string.Empty;
            DrawFilename = string.Empty;
            DrawExtension = ".jpeg";
        }

        public void Run()
        {
            var map = Map.Io.Read(Filename);
            if (map == null || map.IsOk == false) return;
            if (IsImport)
            {
                var imp = Map.Io.Read(ImportFile);
                ResizeMap(map, NewMax);
                if (MergeFrom(map, imp, OffsetPos, FilterRange))
                {
                    Map.Io.Write(Filename, map);
                }
            }
            if (IsDrawMapValues)
            {
                DrawMapValues(map, Filename, DrawFilename, DrawExtension);
            }
            if (IsCreateHexNumbers)
            {
                CreateHexNumbers(map, Filename);
            }
            if (IsCreateHexContours)
            {
                CreateHexContours(map, Filename);
            }
            if (IsCreateElevation)
            {
                CreateElevation(map, Filename);
            }

            if (ExtractMaps.Count != 0)
            {
                using (var source = new Canvas(Filename))
                {
                    source.Open();
                    if (source.IsOpen)
                    {
                        foreach (var extractMap in ExtractMaps)
                        {
                            var rect = source.GetRectangle(extractMap.StartPos, extractMap.MapSize, source.MaxSize);
                            using (var canvas = Canvas.Clone(source, extractMap.Filename, rect))
                            {
                                if (canvas.Save())
                                {
                                    var exp = Map.Io.Read(Filename);
                                    ResizeMap(exp, extractMap.MapSize);
                                    if (CloneTo(exp.MapLocations, map.MapLocations, extractMap.GetOffset()))
                                    {
                                        Map.Io.Write(extractMap.Filename, exp);
                                        if (extractMap.IsDrawMapValues)
                                        {
                                            DrawMapValues(exp, extractMap.Filename, string.Empty, ".jpeg");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawMapValues(Map map, String filename, String drawFilename, String drawExtension)
        {
            var text = new MapValues();
            using (var canvas = new Canvas(filename, drawFilename, drawExtension))
            {
                if (canvas.Graphics != null)
                {
                    text.DrawHexNumbers(canvas.Graphics, map);
                    text.DrawMapValues(canvas.Graphics, map);
                    canvas.Save();
                }
            }
        }

        private void CreateHexNumbers(Map map, String filename)
        {
            var draw = new MapValues();
            using (var canvas = new Canvas(filename, null, ".hexnumbers.png"))
            {
                if (canvas.Graphics != null)
                {
                    canvas.Graphics.Clear(Color.FromArgb(1, Color.Black));
                    draw.DrawHexNumbers(canvas.Graphics, map);
                    canvas.Save();
                }
            }
        }

        private void CreateHexContours(Map map, String filename)
        {
            var draw = new MapValues();
            using (var canvas = new Canvas(filename, null, ".hexcontours.png"))
            {
                if (canvas.Graphics != null)
                {
                    canvas.Graphics.Clear(Color.FromArgb(0, Color.White));
                    draw.DrawHexContours(canvas.Graphics, map);
                    canvas.Save();
                }
            }
        }

        private void CreateElevation(Map map, String filename)
        {
            var draw = new MapValues();
            using (var canvas = new Canvas(filename, null, ".elevation.png"))
            {
                if (canvas.Graphics != null)
                {
                    canvas.Graphics.Clear(Color.Black);
                    draw.DrawElevation(canvas.Graphics, map);
                    canvas.Save();
                }
            }
        }

        private bool ResizeMap(Map map, Map.Pos newMax)
        {
            if (map.IsOk == false) return false;
            if (newMax.Col < map.MapLocations.Min.Col && newMax.Col != 0) return false;
            if (newMax.Row < map.MapLocations.Min.Row && newMax.Row != 0) return false;
            if (newMax.Col == map.MapLocations.Max.Col 
             && newMax.Row == map.MapLocations.Max.Row) return false;
            var newRange = new Map.Range(map.MapLocations.Min, new Map.Pos(newMax.Col != 0 ? newMax.Col : map.MapLocations.Max.Col, newMax.Row != 0 ? newMax.Row : map.MapLocations.Max.Row));
            var newMap = new Map.Locations(newRange);
            newMap.FillWithDefaults();
            if (MergeFrom(newMap, map.MapLocations, new Map.Pos(), new Map.Range()) == false) return false;
            map.MapLocations = newMap;
            return true;
        }

        public bool MergeFrom(Map map, Map imp, Map.Pos offset, Map.Range filter)
        {
            if (imp.IsOk == false) return false;
            if (map.IsOk == false) return false;
            if (MergeFrom(map.MapLocations, imp.MapLocations, offset, filter) == false) return false;
            map.ModifiedDate = DateTime.Now;
            return true;
        }

        public bool MergeFrom(Map.Locations mapLocs, Map.Locations impLocs, Map.Pos offset, Map.Range filter)
        {
            var mapRange = NewMapRange(mapLocs, offset, filter);
            var impRange = NewMapRange(impLocs, new Map.Pos(), filter);
            var mapPos = new Map.Pos();
            var impPos = new Map.Pos();
            var locs = 0;
            for (mapPos.Col = mapRange.Min.Col, impPos.Col = impRange.Min.Col;
                 mapPos.Col <= mapRange.Max.Col && impPos.Col <= impRange.Max.Col;
                 mapPos.Col++, impPos.Col++)
            {
                for (mapPos.Row = mapRange.Min.Row, impPos.Row = impRange.Min.Row;
                     mapPos.Row <= mapRange.Max.Row && impPos.Row <= impRange.Max.Row;
                     mapPos.Row++, impPos.Row++)
                {
                    if (mapLocs.WithinRange(mapPos) &&
                        impLocs.WithinRange(impPos))
                    {
                        mapLocs.Square[mapPos.Col][mapPos.Row] = new Map.Location(mapPos, impLocs.Square[impPos.Col][impPos.Row]);
                        locs++;
                    }
                }
            }
            return locs != 0;
        }

        public bool CloneTo(Map.Locations mapLocs, Map.Locations impLocs, Map.Pos offset)
        {
            var useRowShift = (IsOdd(offset.Col));
            var mapRange = NewMapRange(mapLocs, new Map.Pos(), new Map.Range());
            var impRange = NewMapRange(impLocs, offset, new Map.Range());
            return Copy(mapLocs, impLocs, mapRange, impRange, useRowShift);
        }


        public static bool Copy(Map.Locations mapLocs, Map.Locations impLocs, Map.Range mapRange, Map.Range impRange, bool useRowShift)
        {
            var mapPos = new Map.Pos();
            var impPos = new Map.Pos(useRowShift);
            var locs = 0;
            for (mapPos.Col = mapRange.Min.Col, impPos.Col = impRange.Min.Col;
                 mapPos.Col <= mapRange.Max.Col && impPos.Col <= impRange.Max.Col;
                 mapPos.Col++, impPos.Col++)
            {
                for (mapPos.Row = mapRange.Min.Row, impPos.Row = impRange.Min.Row;
                     mapPos.Row <= mapRange.Max.Row && impPos.Row <= impRange.Max.Row;
                     mapPos.Row++, impPos.Row++)
                {
                    if (mapLocs.WithinRange(mapPos) &&
                        impLocs.WithinRange(impPos))
                    {
                        var impRow = impPos.ShiftedRow;
                        var impCol = impPos.Col;
                        mapLocs.Square[mapPos.Col][mapPos.Row] = new Map.Location(mapPos, impLocs.Square[impCol][impRow]);
                        locs++;
                    }
                }
            }
            return locs != 0;
        }

        public Map.Range NewMapRange(Map.Range map, Map.Pos offset, Map.Range filter)
        
        {
            var range = new Map.Range();
            range.Min.Col = Math.Max((filter.Min.Col != 0 ? filter.Min.Col : map.Min.Col) + offset.Col, map.Min.Col + offset.Col);
            range.Max.Col = Math.Min((filter.Max.Col != 0 ? filter.Max.Col : map.Max.Col) + offset.Col, map.Max.Col + offset.Col);
            range.Min.Row = Math.Max((filter.Min.Row != 0 ? filter.Min.Row : map.Min.Row) + offset.Row, map.Min.Row + offset.Row);
            range.Max.Row = Math.Min((filter.Max.Row != 0 ? filter.Max.Row : map.Max.Row) + offset.Row, map.Max.Row + offset.Row);
            return range;
        }


        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public static bool IsEven(int value)
        {
            return value % 2 == 0;
        }
    }
}
