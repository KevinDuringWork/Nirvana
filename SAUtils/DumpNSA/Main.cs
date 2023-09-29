using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.DumpNSA
{
    public static class Main 
    {

        private static string _nsa;

        public static ExitCodes Run(string command, string[] commandArgs) {
            var ops = new OptionSet 
            {
                {
                    "nsa|n=",
                    "input nsa file",
                    v => _nsa = v
                }
            };
        
            string commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .HasRequiredParameter(_nsa, "input NSA file", "--nsa")
                .CheckInputFilenameExists(_nsa, "input NSA file," , "--nsa")
                .SkipBanner()
                .ShowHelpMenu("Dumps an NSA file", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution() 
        {

            using (var _nsaStream = FileUtilities.GetCreateStream(_nsa))
            using (var _nsaStreamIndex = FileUtilities.GetCreateStream(Path.Combine(_nsa, SaCommon.IndexSuffix)))
            using (var _nsareader = new NsaReader(_nsaStream, _nsaStreamIndex, 1024))
            {
                // System.Console.WriteLine("Hello World!");
                var annotations = new List<(string refAllele, string altAllele, string annotation)>();

                // chr1 ~ 285_000_000
                _nsareader.GetAnnotation(1000, annotations);

                System.Console.WriteLine(annotations);

            }

            return ExitCodes.Success;
        }
    }
}