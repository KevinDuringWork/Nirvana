﻿using System.Collections.Generic;
using CommandLine.Builders;
using ExtractMiniXml;
using SAUtils.CreateGnomadTsv;
using SAUtils.CreateIntermediateTsvs;
using SAUtils.CreateOmimTsv;
using SAUtils.ExtractMiniSa;
using SAUtils.GeneScoresTsv;
using SAUtils.MergeInterimTsvs;
using VariantAnnotation.Interface;

namespace SAUtils
{
    public static class SaUtils 
    {

        public static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["createSA"] = new TopLevelOption("create Nirvana supplementary annotation database", MergeIntermediateTsvs.Run),
                ["createTSV"] = new TopLevelOption("create intermediate tsv file for supplementary annotation", CreateIntermediateTsvsMain.Run),
                ["createOmimTsv"] = new TopLevelOption("create omim tsv file", CreateOmimTsvMain.Run),
                ["geneScoresTsv"] = new TopLevelOption("create gene scores tsv file", GeneScoresMain.Run),
                ["extractMiniSA"] = new TopLevelOption("extracts mini SA", ExtractMiniSaMain.Run),
                ["extractMiniXml"] = new TopLevelOption("extracts mini SA", ExtractMiniXmlMain.Run),
                ["createGnomadTsv"] = new TopLevelOption("create gnomAD tsv file", CreateGnomadTsvMain.Run)
            };


            var exitCode = new TopLevelAppBuilder(args,ops)
                .Parse()
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Utilities focused on supplementary annotation")
                .ShowErrors().Execute();
            return (int) exitCode;
        }
    }
}
