using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqGraph
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var equations = ParserService.ParseSite();
            var vectorizer = new VectorizationService();
            LinkingService preprocessService = new LinkingService(vectorizer);
            preprocessService.LinkWithOntology(equations);
            RdfGenerationService.CreateTriples();
            Console.ReadKey();
        }
    }
}
