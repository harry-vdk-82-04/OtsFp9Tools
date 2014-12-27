using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Ots.cmd
{
    public static class Parcer
    {
        public static Command ReadCommandLine()
        {
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
                            command.IsDrawMapValues = true;
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
                            command.IsDrawMapValues = true;
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
                        default:
                            nrOfOptions--;
                            break;
                    }
                    nrOfOptions++;
                }
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
        }
    }
}
