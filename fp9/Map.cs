using System;
using System.Collections.Generic;
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

            public Pos()
            {
                Col = 0;
                Row = 0;
            }

            public Pos(Pos pos)
            {
                Col = pos.Col;
                Row = pos.Row;
            }

            public override string ToString()
            {
                return string.Format("Col={0}, Row={1}", Col, Row);
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
                       && pos.Row >= Min.Row
                       && pos.Row <= Max.Row;
            }

            public override string ToString()
            {
                return string.Format("Min={0}, Max={1}", Min, Max);
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
        }

        public const int EndOfOtsMarker = 0x29A;
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
            return string.Format("Name={0}, Creator={1}, Map={2}", Name, CreatorName, MapLocations);
        }

        public static class Io
        {
            public static Map Read(string filename)
            {
                var map = new Map();
                using (var stream = File.OpenRead(filename))
                {
                    using (var reader = new BinaryReader(stream, Encoding.Unicode))
                    {
                        map.VersionId = reader.ReadInt32();
                        map.Name = ReadString(reader);
                        map.MapLocations = new Locations(ReadRange(reader));
                        map.MaxLayers = reader.ReadInt32();
                        map.CreatorName = ReadString(reader);
                        map.CreatorDate = ReadDateTime(reader);
                        map.ModifiedDate = ReadDateTime(reader);
                        map.CreatorPw = ReadString(reader);
                        map.Description = ReadString(reader);
                        var pos = new Pos();
                        for (pos.Col = map.MapLocations.Min.Col; pos.Col <= map.MapLocations.Max.Col; pos.Col++)
                        {
                            for (pos.Row = map.MapLocations.Min.Row; pos.Row <= map.MapLocations.Max.Row; pos.Row++)
                            {
                                map.MapLocations.Square[pos.Col][pos.Row] = ReadLocation(reader, pos);
                            }
                        }
                        map.IsOk = (reader.ReadInt32() == EndOfOtsMarker);
                        Console.Out.WriteLine("- Read:"+ Path.GetFileName(filename) +" = OK.");
                    }
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
