using System;
using System.Collections.Generic;
using System.Linq;

namespace EqGraph
{
    public class Equation
    {
        public string URI { get; set; }
        public List<Label> Labels { get; set; }
        public List<string> Types { get; set; }
        public string SubClassOf { get; set; }
        public string LinkedOmpConcept { get; set; }
        public string StringForm { get; set; }
        public string Reference { get; set; }
        public List<string> Comments { get; set; }
        public string LatexForm { get; set; }
        public string HtmlForm { get; set; }

        public Equation()
        {

        }

        public Equation(IEnumerable<string> types,
            string eqWorldRef,
            string label,
            string stringForm,
            string latexForm,
            string hTMLForm)
        {
            URI = Guid.NewGuid().ToString();
            Labels = new List<Label>() { new Label(label, "ru") };
            Types = types.ToList();
            StringForm = stringForm;
            Reference = eqWorldRef;
            LatexForm = latexForm;
            HtmlForm = hTMLForm;
        }
    }
}
