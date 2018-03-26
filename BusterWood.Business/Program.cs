using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Business
{
    static class Program
    {
        public static int Main(string[] argv)
        {
            var args = argv.ToList();
            if (args.Count == 0)
            {
                LogError("Syntax: [--out folder] [--txn] [--async] model-file");
                return 1;
            }

            var txns = args.Remove("--txn");
            var async = args.Remove("--async");

            var @namespace = StringArg(args, "--namespace");

            var outputFolder = StringArg(args, "--out", Environment.CurrentDirectory);
            if (outputFolder == null)
            {
                LogError("--out requires an output folder to follow it");
                return 1;
            }

            if (args.Count == 0)
            {
                LogError("input model file name/path is required");
                return 1;
            }

            string input = args[0];
            using (var reader = new StreamReader(input))
            {
                var mod = Model.Parse(reader);
                var gen = new CsStateMachineGenerator() { Transactions = txns, Async = async };
                gen.Generate(mod, outputFolder, new Dictionary<string, object> { { "namespace", @namespace } });
            }

            return 0;
        }

        private static string StringArg(List<string> args, string arg, string @default = null)
        {
            var idx = args.IndexOf(arg);
            if (idx < 0)
                return @default;

            var result = @default;
            args.RemoveAt(idx);
            if (idx == args.Count)
                return null; // arg name without following parameter
            result = args[idx];
            args.RemoveAt(idx);
            return result;
        }

        private static void LogError(string err)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("BusterWood.Business: " + err);
            Console.ForegroundColor = old;
        }
    }
}
