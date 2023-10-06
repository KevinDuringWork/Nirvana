using System;
using System.Linq;
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
using Genome;

namespace SAUtils.DumpNSA
{
    public static class Main 
    {

        private static string _nsa;

        // ... taken from ChromosomeUtilities in UnitTests
        // added GRCh38: https://www.ncbi.nlm.nih.gov/grc/human/data

        public static readonly Chromosome Chr1  = new Chromosome("chr1", "1", "", "", 248_956_422, 0);
        public static readonly Chromosome Chr2  = new Chromosome("chr2", "2", "", "", 242_193_529, 1);
        public static readonly Chromosome Chr3  = new Chromosome("chr3", "3", "", "", 198_295_559, 2);
        public static readonly Chromosome Chr4  = new Chromosome("chr4", "4", "", "", 190_214_555, 3);
        public static readonly Chromosome Chr5  = new Chromosome("chr5", "5", "", "", 181_538_259, 4);
        public static readonly Chromosome Chr6  = new Chromosome("chr6", "6", "", "", 170_805_979, 5);
        public static readonly Chromosome Chr7  = new Chromosome("chr7", "7", "", "", 159_345_973, 6);
        public static readonly Chromosome Chr8  = new Chromosome("chr8", "8", "", "", 145_138_636, 7);
        public static readonly Chromosome Chr9  = new Chromosome("chr9", "9", "", "", 138_394_717, 8);
        public static readonly Chromosome Chr10 = new Chromosome("chr10", "10", "", "", 133_797_422, 9);
        public static readonly Chromosome Chr11 = new Chromosome("chr11", "11", "", "", 135_086_622, 10);
        public static readonly Chromosome Chr12 = new Chromosome("chr12", "12", "", "", 133_275_309, 11);
        public static readonly Chromosome Chr13 = new Chromosome("chr13", "13", "", "", 114_364_328, 12);
        public static readonly Chromosome Chr14 = new Chromosome("chr14", "14", "", "", 107_043_718, 13);
        public static readonly Chromosome Chr15 = new Chromosome("chr15", "15", "", "", 101_991_189, 14);
        public static readonly Chromosome Chr16 = new Chromosome("chr16", "16", "", "", 90_338_345, 15);
        public static readonly Chromosome Chr17 = new Chromosome("chr17", "17", "", "", 83_257_441, 16);
        public static readonly Chromosome Chr18 = new Chromosome("chr18", "18", "", "", 80_373_285, 17);
        public static readonly Chromosome Chr19 = new Chromosome("chr19", "19", "", "", 58_617_616, 18);
        public static readonly Chromosome Chr20 = new Chromosome("chr20", "20", "", "", 64_444_167, 19);
        public static readonly Chromosome Chr21 = new Chromosome("chr21", "21", "", "", 46_709_983, 20);
        public static readonly Chromosome Chr22 = new Chromosome("chr22", "22", "", "", 50_818_468, 21);
        public static readonly Chromosome ChrX  = new Chromosome("chrX", "X", "", "", 156_040_895, 22);
        public static readonly Chromosome ChrY  = new Chromosome("chrY", "Y", "", "", 57_227_415, 23);
        public static readonly Chromosome ChrM  = new Chromosome("chrM", "MT", "", "", 16_569, 24);
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

            /* 
                We are attempting to dump the binary formats of nsa / gsa into raw text (json) formats 
                for interoperability with external tools. In this case we trust Nirvana's preprocessing 
                of NSA / GSA files and intend to preserve that effort. 

                Much of the code is from "WriterReaderTests.cs" in which Unit tests are construsted 
                to test the readers and writers of the NSA / GSA formats. 

                Steps are as follows:

                1) Build: dotnet-sdk build 
                2) Download References: ./bin/Debug/net6.0/Downloader --ga GRCh38 -o ~/projects/NirvanaData
                3) Dump NSA Reference: ./bin/Debug/net6.0/^CUtils Dump --nsa ~/projects/NirvanaData/SupplementaryAnnotation/GRCh38/ClinVar_20230822.nsa
            */ 

               using (var _nsaStream = FileUtilities.GetReadStream(_nsa))
               using (var _nsaStreamIndex = FileUtilities.GetReadStream(_nsa + SaCommon.IndexSuffix))
               using (var _nsareader = new NsaReader(_nsaStream, _nsaStreamIndex))
               {

                Chromosome[] chromosomes =
                {
                    Chr1, Chr2, Chr3, Chr4, Chr5, Chr6, Chr7, Chr8, Chr9, Chr10, 
                    Chr11, Chr12, Chr13, Chr14, Chr15, Chr16,Chr17, Chr18, Chr19, 
                    Chr20, Chr21, Chr22, ChrX, ChrY, ChrM
                };

                // 1) NSAReader :: Preload (chrom, ...list of positions... )
                // 2) create a buffer and pass in a buffer
                // 3) dump buffer 
                var buf_size = 64 * 1024;

                foreach (Chromosome chrom in chromosomes) {
                    System.Console.WriteLine("{0}:{1}", chrom.UcscName, chrom.Length);
                
                    // + 1 to address remainder
                    for (int i = 0; i < (chrom.Length / buf_size) + 1; i++) {
                        
                        var range = Enumerable.Range(
                            i*buf_size + 1, 
                            Math.Max((i+1)*buf_size + 1, chrom.Length + 1)
                        ).ToList();
                        
                        // perform preload of chromosome + buffered range 
                        _nsareader.PreLoad(chrom, range);

                        // dump each preloaded into typed list of tuples 
                        for (int p = i*buf_size; p < (i+i)*buf_size; p++) {
                            var annotations = new List<(string refAllele, string altAllele, string annotation)>();
                            _nsareader.GetAnnotation(p, annotations);
                            annotations.ForEach(a => Console.WriteLine(a));
                        }

                    }

                }
            }

            return ExitCodes.Success;
        }
    }
}