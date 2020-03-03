﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.Omim.EntryApiResponse;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;

namespace SAUtils.Omim
{
    public static class OmimUtilities
    {
        public static OmimItem.Phenotype GetPhenotype(PhenotypeMap phenotypeMap, SaJsonSchema jsonSchema)
        {
            var phenotypeItem = phenotypeMap.phenotypeMap;

            var (phenotype, comments) = ExtractPhenotypeAndComments(phenotypeItem.phenotype);
            return new OmimItem.Phenotype(phenotypeItem.phenotypeMimNumber, phenotype, (OmimItem.Mapping)phenotypeItem.phenotypeMappingKey, comments, ExtractInheritances(phenotypeItem.phenotypeInheritance), jsonSchema);
        }

        private static HashSet<string> ExtractInheritances(string inheritance)
        {
            var inheritances = new HashSet<string>();
            if (string.IsNullOrEmpty(inheritance)) return inheritances;

            foreach (string content in inheritance.OptimizedSplit(';'))
            {
                string trimmedContent = content.Trim(' ');
                inheritances.Add(trimmedContent);
            }

            return inheritances;
        }

        internal static (string Phenotype, OmimItem.Comment[] Comments) ExtractPhenotypeAndComments(string phenotypeString)
        {
            phenotypeString = phenotypeString.Trim(' ').Trim(',').Replace(@"\\'", "'", StringComparison.Ordinal);
            string phenotype = Regex.Replace(phenotypeString,@" \(\d\) ", " ");

            var comments = phenotypeString.Select(GetComment)
                                          .Where(x => x != OmimItem.Comment.unknown)
                                          .ToArray();

            return (phenotype, comments);
        }

        private static OmimItem.Comment GetComment(char symbol)
        {
            switch (symbol)
            { 
                case '?':
                    return OmimItem.Comment.unconfirmed_or_possibly_spurious_mapping;
                case '[':
                    return OmimItem.Comment.nondiseases;
                case '{':
                    return OmimItem.Comment.contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection;
                default:
                    return OmimItem.Comment.unknown;
            }
        }

        public static Dictionary<string, List<ISuppGeneItem>> GetGeneToOmimEntriesAndSchema(IEnumerable<OmimItem> omimItems)
        {
            var geneToOmimEntries = new Dictionary<string, List<ISuppGeneItem>>();
            SaJsonSchema jsonSchema = null;

            foreach (var item in omimItems)
            {
                if (jsonSchema == null) jsonSchema = item.JsonSchema;
                if (item.GeneSymbol == null) continue;

                if (geneToOmimEntries.TryGetValue(item.GeneSymbol, out var mimList))
                {
                    mimList.Add(item);
                }
                else
                {
                    geneToOmimEntries[item.GeneSymbol] = new List<ISuppGeneItem> { item };
                }
            }

            return geneToOmimEntries;
        }

        // remove links enclosed by parentheses with only numbers, e.g. ({12345})
        public static string RemoveLinks(this string text) => text == null
            ? null
            : Regex.Replace(Regex.Replace(Regex.Replace(text, 
                        @"((and|see|;|(e\.g\.)?,) )*{\d+(\.\d+)?}", ""),
                        @" ?\((\ |/)*\)", ""),
                        @"{(\d+:)?(.+?)}", "$2");

        public static string RemoveFormatControl(this string text) => text == null ? null : 
            Regex.Replace(text, "<Subhead>", "");
    }
}