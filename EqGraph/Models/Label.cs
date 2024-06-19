using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqGraph
{
    public class Label
    {
        public string LabelText { get; set; }
        public string LangTag { get; set; }

        public Label()
        {
            
        }

        public Label(string labelText, string langTag)
        {
            LabelText = labelText;
            LangTag = langTag;
        }
    }
}
