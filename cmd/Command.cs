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
        public Map.Pos NewMax { get; set; }
        public String Filename { get; set; }
        public String ImportFile { get; set; }

        public Command()
        {
            Cmd = CmdType.IsUnknown;
            OffsetPos = new Map.Pos();
            NewMax = new Map.Pos();
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
                    ResizeMap(map, NewMax);
                    if (MergeFrom(map, imp, OffsetPos, FilterRange))
                    {
                        Map.Io.Write(Filename, map);
                    }
                }
                    break;
            }
        }

        private bool ResizeMap(Map map, Map.Pos newMax)
        {
            if (map.IsOk == false) return false;
            if (newMax.Col < map.MapLocations.Min.Col) return false;
            if (newMax.Row < map.MapLocations.Min.Row) return false;
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

        public bool MergeFrom(Map.Locations map, Map.Locations imp, Map.Pos offset, Map.Range filter)
        {
            var mapRange = NewMapRange(map, offset, filter);
            var impRange = NewMapRange(imp, new Map.Pos(), filter);
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
                    map.Square[mapPos.Col][mapPos.Row] = new Map.Location(mapPos, imp.Square[impPos.Col][impPos.Row]);
                    locs++;
                }
            }
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
