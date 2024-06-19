using System;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace EqGraph
{
    public static class RdfGenerationService
    {
        public static void CreateTriples()
        {
            Console.WriteLine("Генерация RDF-триплетов и создание графа знаний...");
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.FileName = "java";
            procInfo.Arguments = $@"-jar rmlmapper.jar -m mapping.ttl -o output.ttl -s turtle";
            var process = Process.Start(procInfo);
            if (process != null)
            {
                process.WaitForExit();
            }
            SparqlService sparqlService = new SparqlService();
            sparqlService.SaveFromFile("output.ttl");
            Console.WriteLine("Граф знаний создан");
        }
    }
}
