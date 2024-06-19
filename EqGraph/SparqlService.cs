using System;
using VDS.RDF.Parsing;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace EqGraph
{
    public class SparqlService
    {
        public Uri EndpointUri = new Uri("http://localhost:8890/sparql/");
        public static string DefaultGraphUri = "http://localhost:8890/OntoMathPro";
        public static string OntologyIRI = "<http://ontomathpro.org/omp2>#";
        public static string OntologyPrefix = "omp2";
        static string graphBaseIri = "http://localhost:8890/EqGraph";
        public SparqlRemoteEndpoint endpoint;


        public SparqlService()
        {
            endpoint = new SparqlRemoteEndpoint(EndpointUri, DefaultGraphUri);

        }

        public SparqlResultSet GetOMPClassInfoByLabel(string label)
        {
            var results = endpoint.QueryWithResultSet(

                "PREFIX omp2: <http://ontomathpro.org/omp2#>" +
                "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                $"SELECT ?s ?p ?o WHERE " +
                $"{{" +
                    $"?s rdfs:subClassOf*/rdfs:subClassOf omp2:E1891 ." +
                    $"?s rdfs:label \"{label}\"@ru ." +
                    $"?s ?p ?o ." +
                $"}} "
            );

            if (results.IsEmpty)
                return null;
            return results;
        }

        public SparqlResultSet GetOMPClassInfoByIri(string iri)
        {
            var results = endpoint.QueryWithResultSet(
               "PREFIX omp2: <http://ontomathpro.org/omp2#>" +
               $"SELECT ?p ?o WHERE " +
               $"{{" +
                   $"omp2:{iri} ?p ?o ." +
               $"}} "
           );

            if (results.IsEmpty)
                return null;
            return results;
        }

        public SparqlResultSet GetOntologySubjects()
        {

            var results = endpoint.QueryWithResultSet(
                "PREFIX omp2: <http://ontomathpro.org/omp2#>" +
                "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                $"SELECT ?s WHERE " +
                $"{{" +
                    $"?s rdfs:subClassOf*/rdfs:subClassOf omp2:E1891 ." +
                $"}} ");
            if (results.IsEmpty)
                return null;
            return results;
        }

        public SparqlResultSet GetAllSubjectTypes(string iri)
        {
            var results = endpoint.QueryWithResultSet(
                "PREFIX omp2: <http://ontomathpro.org/omp2#>" +
                "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                "SELECT ?label WHERE " +
                "{" +
                    $"{{{iri} rdfs:label ?label  FILTER (lang(?label)=\"ru\")}}" +
                    $"UNION" +
                    "{" +
                        $"{iri} rdfs:subClassOf+ ?o ." +
                        $"?o rdfs:subClassOf*/rdfs:subClassOf omp2:E1891 ." +
                        "?o rdfs:label ?label  FILTER (lang(?label)=\"ru\")." +
                    "}" +
                "}");

            if (results.IsEmpty)
                return null;
            return results;
        }

        public SparqlResultSet GetSubjectLabel(string iri)
        {
            var results = endpoint.QueryWithResultSet(
                "PREFIX omp2: <http://ontomathpro.org/omp2#>" +
                "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                "SELECT ?label WHERE " +
                "{" +
                    $"{{{iri} rdfs:label ?label  FILTER (lang(?label)=\"ru\")}}" +
                "}");

            if (results.IsEmpty)
                return null;
            return results;
        }

        public void SaveFromFile(string fileName)
        {
            VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, VirtuosoManager.DefaultDB, "dba", "dba");

            IGraph g = new Graph();
            TurtleParser ttlParser = new TurtleParser();

            ttlParser.Load(g, "output.ttl");
            g.BaseUri = new Uri(graphBaseIri);
            virtuoso.SaveGraph(g);
        }

    }
}
