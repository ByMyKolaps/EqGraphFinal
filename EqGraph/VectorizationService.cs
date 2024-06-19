using Accord.MachineLearning;
using Accord.Math;
using Accord.Math.Distances;
using EqGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace EqGraph
{
    public class VectorizationService
    {
        Dictionary<string, double[]> OntologyConceptsDocumentMatrix;
        TFIDF TF_IDF_Model;

        public VectorizationService()
        {
            CreateOntologyConceptMatrix();
        }

        void CreateOntologyConceptMatrix()
        {
            Dictionary<string, List<string>> uriTokensMap = new Dictionary<string, List<string>>();

            var sparqlService = new SparqlService();
            var subjects = sparqlService.GetOntologySubjects();

            foreach (var subject in subjects)
            {
                HashSet<string> tokens = new HashSet<string>();

                var iri = (IUriNode)subject.Value("s");
                var iriFragment = iri.Uri.Fragment.Replace("#", "");

                if (uriTokensMap.ContainsKey(iriFragment))
                    continue;

                var labels = sparqlService.GetSubjectLabel($"omp2:{iriFragment}");

                foreach (var label in labels)
                {
                    var labelStr = (ILiteralNode)label.Value("label");
                    var tags = TextPreprocesser.ApplyPreprocess(labelStr.Value);
                    foreach (var tag in tags)
                    {
                        if (!tokens.Contains(tag))
                            tokens.Add(tag);
                    }
                }
                uriTokensMap.Add(iriFragment, tokens.ToList());
            }

            CreateTF_IDF(uriTokensMap);
        }

        void CreateTF_IDF(Dictionary<string, List<string>> tags)
        {
            var documentMatrix = CreateDocumentMatrix(tags); 

            var tf_idf = new TFIDF();
            tf_idf.Learn(documentMatrix);

            TF_IDF_Model = tf_idf;
            OntologyConceptsDocumentMatrix = MapConceptUriToVector(tags.Keys.ToList(), documentMatrix);
        }

        string[][] CreateDocumentMatrix(Dictionary<string, List<string>> tags)
        {
            string[][] result = new string[tags.Count][];
            for (int i = 0; i < tags.Count; i++)
            {
                result[i] = tags.ElementAt(i).Value.ToArray();
            }
            return result;
        }

        Dictionary<string, double[]> MapConceptUriToVector(List<string> iris, string[][] documentMatrix)
        {
            var map = new Dictionary<string, double[]>();
            var j = 0;
            foreach (var iri in iris)
            {
                var vector = TF_IDF_Model.Transform(documentMatrix[j]);
                j++;
                map.Add(iri, vector);
            }
            return map;
        }

        static string[] GetEquationTags(Equation equation)
        {
            HashSet<string> tags = new HashSet<string>();
            if (equation.Labels[0].LabelText != "Без названия")
            {
                var tokens = TextPreprocesser.ApplyPreprocess(equation.Labels[0].LabelText);
                foreach (var token in tokens)
                    tags.Add(token);
            }
            foreach (var eqType in equation.Types)
            {
                var tokens = TextPreprocesser.ApplyPreprocess(eqType);
                foreach (var token in tokens)
                    tags.Add(token);
            }
            return tags.ToArray();
        }

        double[] CreateVector(string[] tags)
        {
            return TF_IDF_Model.Transform(tags);
        }

        public string GetClosestConceptUri(Equation equation)
        {
            var eqTags = GetEquationTags(equation);
            var eqTagsVector = CreateVector(eqTags);

            Dictionary<string, double> similarity = new Dictionary<string, double>();
            Cosine c = new Cosine();
            foreach (var concept in OntologyConceptsDocumentMatrix)
            {
                similarity.Add(concept.Key, c.Similarity(eqTagsVector, concept.Value));
            }
            var closestConcept = similarity.OrderByDescending(t => t.Value).First();
            return closestConcept.Key;
        }
    }
}
