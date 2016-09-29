using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Ots.cmd;

namespace Ots.fp9
{
    public class Map
    {
        public class Pos
        {
            public int Col { get; set; }
            public int Row { get; set; }

            public int ShiftedRow
            {
                get
                {
                    return (UseRowShift && IsOdd(Col) ? Row + 1 : Row);
                }
            }

            public bool UseRowShift { get; set; }

            public bool IsRowEven => IsEven(Row);

            public bool IsRowOdd => IsOdd(Row);

            public bool IsColEven => IsEven(Col);

            public bool IsColOdd => IsOdd(Col);

            public Pos(bool useRowShift = false)
            {
                Col = 0;
                Row = 0;
                UseRowShift = useRowShift;
            }

            public Pos(Pos pos, bool? useRowShift = null)
            {
                Col = pos.Col;
                Row = pos.Row;
                UseRowShift = (bool) (useRowShift ?? pos.UseRowShift);
            }

            public Pos(int col, int row)
            {
                Col = col;
                Row = row;
                UseRowShift = false;
            }

            public override string ToString()
            {
                return string.Format("Col={0}, Row={1}", Col, Row);
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

        public class Neighbours : Pos
        {
            public PointF Point1 { get; }
            public PointF Point2 { get; }
            public Color Color { get; }
            public Neighbours(Pos pos, PointF point1, PointF point2, Color color) : base(pos)
            {
                Point1 = point1;
                Point2 = point2;
                Color = color;
            }
        }

        public class Range
        {
            public Pos Min { get; set; }
            public Pos Max { get; set; }

            public Range()
            {
                Min = new Pos();
                Max = new Pos();
            }

            public Range(Range range)
            {
                Min = new Pos(range.Min);
                Max = new Pos(range.Max);
            }

            public Range(Pos min, Pos max)
            {
                Min = new Pos(min);
                Max = new Pos(max);
            }

            public bool WithinRange(Pos pos)
            {
                return pos.Col >= Min.Col
                       && pos.Col <= Max.Col
                       && pos.ShiftedRow >= Min.Row
                       && pos.ShiftedRow <= Max.Row;
            }

            public override string ToString()
            {
                return string.Format("Min={0}, Max={1}", Min, Max);
            }

            public static bool IsOdd(int value)
            {
                return value % 2 != 0;
            }
        }

        public class Location
        {
            public Pos LocPos { get; protected set; }
            public int Elevation { get; set; }
            public int MobilityRating { get; set; }
            public int CoverRating { get; set; }
            public int VisibilityRating { get; set; }
            public int Defensibility { get; set; }
            public int Reach { get; set; }
            public int Visibility { get; set; }
            public string ObstacleCode { get; set; }
            public string RoadCode { get; set; }
            public string PlaceName { get; set; }
            public int SetupOwnership { get; set; }
            public string Description { get; set; }
            public bool IsOk { get; set; }

            public Location(Pos pos)
            {
                LocPos = new Pos(pos);
                Elevation = 8;
                MobilityRating = 7;
                CoverRating = 7;
                VisibilityRating = 0;
                Defensibility = 0;
                Reach = 0;
                Visibility = 3;
                ObstacleCode = "0";
                RoadCode = "";
                PlaceName = "";
                SetupOwnership = -1;
                Description = "Elev 8, Forest, ";
            }

            public Location(Pos pos, Location source)
            {
                LocPos = new Pos(pos);
                Elevation = source.Elevation;
                MobilityRating = source.MobilityRating;
                CoverRating = source.CoverRating;
                VisibilityRating = source.VisibilityRating;
                Defensibility = source.Defensibility;
                Reach = source.Reach;
                Visibility = source.Visibility;
                ObstacleCode = source.ObstacleCode;
                RoadCode = source.RoadCode;
                PlaceName = source.PlaceName;
                SetupOwnership = source.SetupOwnership;
                Description = source.Description;
                IsOk = true;
            }
        }

        public class Locations : Range
        {
            public Location[][] Square { get; set; }

            public Locations(Range range) : base(range)
            {
                Square = new Location[Max.Col+1][];
                for (var col = Min.Col; col <= Max.Col; col++)
                {
                    Square[col] = new Location[Max.Row + 1];
                }
            }

            public void FillWithDefaults()
            {
                var pos = new Map.Pos();
                for (pos.Col = Min.Col; pos.Col <= Max.Col; pos.Col++)
                {
                    for (pos.Row = Min.Row; pos.Row <= Max.Row; pos.Row++)
                    {
                        if (Square[pos.Col][pos.Row] == null) Square[pos.Col][pos.Row] = new Location(pos);
                    }
                }
            }

            public IEnumerable<Location> GetAllLocations()
            {
                var pos = new Map.Pos();
                for (pos.Col = Min.Col; pos.Col <= Max.Col; pos.Col++)
                {
                    for (pos.Row = Min.Row; pos.Row <= Max.Row; pos.Row++)
                    {
                        yield return Square[pos.Col][pos.Row];
                    }
                }
            }
        }

        public const int EndOfOtsMarker = 0x29A;
        public const int Fp9VersionId = 0x12E;
        public int VersionId { get; set; }
        public string Name { get; set; }
        public int MaxLayers { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreatorDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string CreatorPw { get; set; }
        public string Description { get; set; }
        public Locations MapLocations { get; set; }
        public bool IsOk { get; set; }


        public override string ToString()
        {
            return string.Format("Name={0}, Creator={1}, IsOk={2}, Map={3}", Name, CreatorName, IsOk, MapLocations);
        }

        public static class Io
        {
            public static Map Read(string filename)
            {
                filename = Path.ChangeExtension(filename, ".fp9");
                var map = new Map();
                try
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        using (var reader = new BinaryReader(stream, Encoding.Unicode))
                        {
                            map.VersionId = reader.ReadInt32();
                            if (Fp9VersionId != map.VersionId) return map;
                            map.Name = ReadString(reader);
                            map.MapLocations = new Locations(ReadRange(reader));
                            map.MaxLayers = reader.ReadInt32();
                            map.CreatorName = ReadString(reader);
                            map.CreatorDate = ReadDateTime(reader);
                            map.ModifiedDate = ReadDateTime(reader);
                            map.CreatorPw = ReadString(reader);
                            map.Description = ReadString(reader);
                            ReadLocations(reader, map);
                            map.IsOk = (reader.ReadInt32() == EndOfOtsMarker);
                            Console.Out.WriteLine("- Read:"+ Path.GetFileName(filename) +" = OK.");
                        }
                    }
                }
                catch (Exception)
                {
                    Console.Out.WriteLine("- Read:" + Path.GetFileName(filename) + " = Failed.");
                }
                return map;
            }

            private static string ReadString(BinaryReader reader)
            {
                var cb = new StringBuilder();
                var strLen = reader.ReadInt32();
                for(var i=1; i<=strLen;i++) cb.Append(reader.ReadChar());
                return cb.ToString();
            }

            private static DateTime ReadDateTime(BinaryReader reader)
            {
                return DateTime.FromOADate(reader.ReadDouble());
            }

            private static Range ReadRange(BinaryReader reader)
            {
                var range = new Range
                { 
                    Min = { Col = reader.ReadInt32(), Row = reader.ReadInt32() },
                    Max = { Col = reader.ReadInt32(), Row = reader.ReadInt32() }
                };
                return range;
            }

            private static void ReadLocations(BinaryReader reader, Map map)
            {
                var pos = new Pos();
                for (pos.Col = map.MapLocations.Min.Col; pos.Col <= map.MapLocations.Max.Col; pos.Col++)
                {
                    for (pos.Row = map.MapLocations.Min.Row; pos.Row <= map.MapLocations.Max.Row; pos.Row++)
                    {
                        map.MapLocations.Square[pos.Col][pos.Row] = ReadLocation(reader, pos);
                    }
                }
            }

            private static Location ReadLocation(BinaryReader reader, Pos pos)
            {
                var loc = new Location(pos)
                {
                    Elevation = reader.ReadInt32(),
                    MobilityRating = reader.ReadInt32(),
                    CoverRating = reader.ReadInt32(),
                    VisibilityRating = reader.ReadInt32(),
                    Defensibility = reader.ReadInt32(),
                    Reach = reader.ReadInt32(),
                    Visibility = reader.ReadInt32(),
                    ObstacleCode = ReadString(reader),
                    RoadCode = ReadString(reader),
                    PlaceName = ReadString(reader),
                    SetupOwnership = reader.ReadInt32(),
                    Description = ReadString(reader),
                    IsOk = (reader.ReadInt32() == EndOfOtsMarker)
                };
                return loc;
            }

            public static void Write(string filename, Map map)
            {
                filename = Path.ChangeExtension(filename, ".fp9");
                if (File.Exists(filename)) File.Delete(filename);
                using (var stream = File.OpenWrite(filename))
                {
                    map.Name = Path.GetFileNameWithoutExtension(filename);
                    using (var writer = new BinaryWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(map.VersionId);
                        WriteString(writer, map.Name);
                        WriteRange(writer, map.MapLocations);
                        writer.Write(map.MaxLayers);
                        WriteString(writer, map.CreatorName);
                        WriteDateTime(writer, map.CreatorDate);
                        WriteDateTime(writer, map.ModifiedDate);
                        WriteString(writer, map.CreatorPw);
                        WriteString(writer, map.Description);
                        var pos = new Pos();
                        for (pos.Col = map.MapLocations.Min.Col; pos.Col <= map.MapLocations.Max.Col; pos.Col++)
                        {
                            for (pos.Row = map.MapLocations.Min.Row; pos.Row <= map.MapLocations.Max.Row; pos.Row++)
                            {
                                WriteLocation(writer, map.MapLocations.Square[pos.Col][pos.Row]);
                            }
                        }
                        writer.Write(EndOfOtsMarker);
                        Console.Out.WriteLine("- Write:" + Path.GetFileName(filename) + " = OK.");
                    }
                }
            }

            private static void WriteString(BinaryWriter writer, string value)
            {
                writer.Write(value.Length);
                foreach (char chr in value)
                    writer.Write(chr);
            }

            private static void WriteDateTime(BinaryWriter writer, DateTime value)
            {
                writer.Write(value.ToOADate());
            }

            private static void WriteRange(BinaryWriter writer, Range value)
            {
                writer.Write(value.Min.Col);
                writer.Write(value.Min.Row);
                writer.Write(value.Max.Col);
                writer.Write(value.Max.Row);
            }

            private static void WriteLocation(BinaryWriter writer, Location loc)
            {
                writer.Write(loc.Elevation);
                writer.Write(loc.MobilityRating);
                writer.Write(loc.CoverRating);
                writer.Write(loc.VisibilityRating);
                writer.Write(loc.Defensibility);
                writer.Write(loc.Reach);
                writer.Write(loc.Visibility);
                WriteString(writer, loc.ObstacleCode);
                WriteString(writer, loc.RoadCode);
                WriteString(writer, loc.PlaceName);
                writer.Write(loc.SetupOwnership);
                WriteString(writer, loc.Description);
                writer.Write(EndOfOtsMarker);
            }
        }
    }
}
