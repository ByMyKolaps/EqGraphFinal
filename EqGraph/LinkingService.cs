using Accord;
using Accord.MachineLearning;
using Accord.MachineLearning.Text.Stemmers;
using Accord.Math;
using Accord.Math.Distances;
using EqGraph.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace EqGraph
{
    public class LinkingService
    {
        VectorizationService Vectorizer;
        SparqlService SparqlService;
        public LinkingService(VectorizationService vectorizer)
        {

            Vectorizer = vectorizer;
            SparqlService = new SparqlService();

        }

        public void LinkWithOntology(List<Equation> equations)
        {
            Console.WriteLine("Разметка в терминах онтологии...");

            foreach (var equation in equations)
            {
                if (TryDirectLinking(equation))
                    continue;
                SimilarityLinking(equation);
                
            }
            SaveAsXML(equations);
        }

        public bool TryDirectLinking(Equation equation)
        {
            SparqlResultSet result = null;
            if (equation.Labels[0].LabelText != "Без названия")
            {
                result = SparqlService.GetOMPClassInfoByLabel(equation.Labels[0].LabelText);
                if (result != null)
                {
                    CompleteEquation(equation, result);
                    return true;
                }
            }
            foreach (var eqType in equation.Types)
            {
                result = SparqlService.GetOMPClassInfoByLabel(eqType);
                if (result != null)
                {
                    CompleteEquation(equation, result);
                    return true;
                }
            }
            return false;
        }

        public void SimilarityLinking(Equation equation)
        {
            var closestConceptUri = Vectorizer.GetClosestConceptUri(equation);
            var closestConceptInfo = SparqlService.GetOMPClassInfoByIri(closestConceptUri);
            CompleteEquation(closestConceptUri, closestConceptInfo, equation);
        }
        
        static void CompleteEquation(Equation equation, SparqlResultSet triplets)
        {
            equation.LinkedOmpConcept = triplets[0].Value("s").ToString();

            equation.SubClassOf = triplets
                .FirstOrDefault(t => new Uri(t.Value("p").ToString()).Fragment == "#subClassOf")
                .Value("o")
                .ToString();

            equation.Comments = triplets
                .Where(t => new Uri(t.Value("p").ToString()).Fragment == "#comment")
                .Select(t => t.Value("o").AsValuedNode().AsString())
                .ToList();

            equation.Labels.AddRange(triplets
                .Where(t => new Uri(t.Value("p").ToString()).Fragment == "#label")
                .Select(t => (ILiteralNode)t.Value("o"))
                .Select(t => new Label(t.Value, t.Language))
                .ToList());
        }

        static void CompleteEquation(string iri, SparqlResultSet triplets, Equation equation)
        {
            equation.LinkedOmpConcept = $"http://ontomathpro.org/omp2#{iri}";
            equation.SubClassOf = triplets
                .FirstOrDefault(t => new Uri(t.Value("p").ToString()).Fragment == "#subClassOf")
                .Value("o")
                .ToString();

            equation.Comments = triplets
                .Where(t => new Uri(t.Value("p").ToString()).Fragment == "#comment")
                .Select(t => t.Value("o").AsValuedNode().AsString())
                .ToList();

            equation.Labels.AddRange(triplets
                .Where(t => new Uri(t.Value("p").ToString()).Fragment == "#label")
                .Select(t => (ILiteralNode)t.Value("o"))
                .Select(t => new Label(t.Value, t.Language))
                .ToList());
        }

        public static void SaveAsXML(List<Equation> equations)
        {
            Console.WriteLine("Сохранение XML-файла...");
            XmlSerializer formatter = new XmlSerializer(typeof(List<Equation>));

            using (FileStream fs = new FileStream("equations.xml", FileMode.Create))
            {
                formatter.Serialize(fs, equations);
            }
        }

    }
}
