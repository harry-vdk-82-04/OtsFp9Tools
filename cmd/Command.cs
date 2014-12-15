using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ots.fp9;

namespace Ots.cmd
{
    public class Command
    {
        public enum CmdType
        {
            IsUnknown = 0,
            IsImport = 1,
        };

        public CmdType Cmd { get; set; }
        public Map.Pos OffsetPos { get; set; }
        public Map.Range FilterRange { get; set; }
        public String Filename { get; set; }
        public String ImportFile { get; set; }

        public Command()
        {
            Cmd = CmdType.IsUnknown;
            OffsetPos = new Map.Pos();
            FilterRange = new Map.Range();
            Filename = string.Empty;
            ImportFile = string.Empty;
        }

        public void Run()
        {
            switch (Cmd)
            {
                case CmdType.IsImport:
                {
                    var map = Map.Io.Read(Filename);
                    var imp = Map.Io.Read(ImportFile);
                    if (MergeFrom(map, imp, OffsetPos, FilterRange))
                    {
                        Map.Io.Write(Filename, map);
                    }
                }
                    break;
            }
        }

        public bool MergeFrom(Map map, Map imp, Map.Pos offset, Map.Range filter)
        {
            if (imp.IsOk == false) return false;
            if (map.IsOk == false) return false;
            var mapRange = NewMapRange(map.MapLocations, offset, filter);
            var impRange = NewMapRange(imp.MapLocations, new Map.Pos(), filter);
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
                    map.MapLocations.Square[mapPos.Col][mapPos.Row] = new Map.Location(mapPos, imp.MapLocations.Square[impPos.Col][impPos.Row]);
                    locs++;
                }
            }
            map.ModifiedDate = DateTime.Now;
            return locs != 0;
        }

        public Map.Range NewMapRange(Map.Range map, Map.Pos offset, Map.Range filter)
        
        {
            var range = new Map.Range();
            range.Min.Col = Math.Max((filter.Min.Col != 0 ? filter.Min.Col : map.Min.Col) + offset.Col, map.Min.Col);
            range.Max.Col = Math.Min((filter.Max.Col != 0 ? filter.Max.Col : map.Max.Col) + offset.Col, map.Max.Col);
            range.Min.Row = Math.Max((filter.Min.Row != 0 ? filter.Min.Row : map.Min.Row) + offset.Row, map.Min.Row);
            range.Max.Row = Math.Min((filter.Max.Row != 0 ? filter.Max.Row : map.Max.Row) + offset.Row, map.Max.Row);
            return range;
        }
    }
}
