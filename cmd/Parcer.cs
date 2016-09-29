using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Ots.fp9;

namespace Ots.cmd
{
    public static class Parcer
    {
        public static Command ReadCommandLine()
        {
            var extractMap = new Command.ExtractMap();
            var separator = new[] { '=' };
            var command = new Command();
            var arguments = Environment.GetCommandLineArgs();
            var doPrintHelp = false;
            var nrOfOptions = 0;
            foreach (var arg in arguments)
            {
                var split = arg.Split(separator);
                if (arg == "/?" || string.CompareOrdinal(arg.ToLower(), "/help") == 0)
                {
                    doPrintHelp = true;
                }
                if (split.GetLength(0) <= 1)
                {
                    switch (arg)
                    {
                        case "/drawmapvalues":
                            if (string.IsNullOrEmpty(extractMap.Filename) == false)
                            {
                                extractMap.IsDrawMapValues = true;
                            }
                            else
                            {
                                command.IsDrawMapValues = true;
                            }
                            break;
                        case "/createhexnumbers":
#if DEBUG
                            command.IsCreateHexNumbers = true;
#endif
                            break;
                        case "/createhexcontours":
                            command.IsCreateHexContours = true;
                            break;
                        case "/createelevation":
                            command.IsCreateElevation = true;
                            break;
                    }
                }
                if (split.GetLength(0) == 2)
                {
                    var cmd = split[0].ToLower();
                    var param = split[1];
                    int intVar;
                    switch (cmd)
                    {
                        case "/filename":
                            command.Filename = param;
                            break;
                        case "/drawmapvalues":
                            if (extractMap.HasFilename)
                            {
                                extractMap.IsDrawMapValues = true;
                            }
                            else
                            {
                                command.IsDrawMapValues = true;
                            }
                            break;
                        case "/drawfilename":
                            command.DrawFilename = param;
                            break;
                        case "/drawextension":
                            command.DrawExtension = param;
                            break;
                        case "/import":
                            command.IsImport = true;
                            command.ImportFile = param;
                            break;
                        case "/extractmap":
                            if (extractMap.HasFilename)
                            {
                                command.ExtractMaps.Add(extractMap);
                                extractMap = new Command.ExtractMap(extractMap);
                            }
                            extractMap.Filename = param;
                            break;
                        case "/offsetcol":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.OffsetPos.Col = intVar;
                            break;
                        case "/offsetrow":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.OffsetPos.Row = intVar;
                            break;
                        case "/rangemincol":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.FilterRange.Min.Col = intVar;
                            break;
                        case "/rangeminrow":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.FilterRange.Min.Row = intVar;
                            break;
                        case "/rangemaxcol":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.FilterRange.Max.Col = intVar;
                            break;
                        case "/rangemaxrow":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.FilterRange.Max.Row = intVar;
                            break;
                        case "/newmaxcol":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.NewMax.Col = intVar;
                            break;
                        case "/newmaxrow":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) command.NewMax.Row = intVar;
                            break;
                        case "/startcol":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) extractMap.StartPos.Col = intVar;
                            break;
                        case "/startrow":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) extractMap.StartPos.Row = intVar;
                            break;
                        case "/nrofcols":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) extractMap.MapSize.Col = intVar;
                            break;
                        case "/nrofrows":
                            if (string.IsNullOrEmpty(param)) break;
                            if (int.TryParse(param, out intVar)) extractMap.MapSize.Row = intVar;
                            break;
                        default:
                            nrOfOptions--;
                            break;
                    }
                    nrOfOptions++;
                }
            }
            if (string.IsNullOrEmpty(extractMap.Filename) == false)
            {
                command.ExtractMaps.Add(extractMap);
            }
            if (doPrintHelp || nrOfOptions == 0)
            {
                PrintHelpToStdOut();
            }
            return command;
        }

        private static void PrintHelpToStdOut()
        {
            Console.Out.WriteLine("Options:");
            Console.Out.WriteLine("/?");
            Console.Out.WriteLine("/filename=<base filename>");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Used for merging mapvalues");
            Console.Out.WriteLine("/import=<import filename>");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("[/OffsetCol=<int>]");
            Console.Out.WriteLine("[/OffsetRow=<int>]");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("[/RangeMinCol=<int>]");
            Console.Out.WriteLine("[/RangeMinRow=<int>]");
            Console.Out.WriteLine("[/RangeMaxCol=<int>]");
            Console.Out.WriteLine("[/RangeMaxRow=<int>]");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("[/NewMaxCol=<int>]");
            Console.Out.WriteLine("[/NewMaxRow=<int>]");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Used for drawing mapvalues");
            Console.Out.WriteLine("/drawmapvalues");
            Console.Out.WriteLine("[/drawfilename=<optional filename>]");
            Console.Out.WriteLine("[/drawextension=<optional extension>]");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Used for extracting map[s]");
            Console.Out.WriteLine("/ExtractMap=<extract to filename>");
            Console.Out.WriteLine("/StartCol=<int>");
            Console.Out.WriteLine("/StartRow=<int>");
            Console.Out.WriteLine("/NrOfCols=<int>{default=46}");
            Console.Out.WriteLine("/NrOfRows=<int>{default=30}");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Used for qgis");
#if DEBUG
            Console.Out.WriteLine("/CreateHexNumbers");
#endif
            Console.Out.WriteLine("/CreateElevation");
            Console.Out.WriteLine("/CreateHexContours");
        }
    }
}
