using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EqGraph
{
    public class EquationFormsBuilder
    {

        static Dictionary<string, string> strReplacements = new Dictionary<string, string>()
        {
            { "&minus;", "-" },
            { "&prime;", "'" },
            { "&lambda;", "\u03BB" },
            { "&alpha;", "\u03B1" },
            { "&beta;", "\u03B2" },
            { "&gamma;", "\u03B3" },
            { "&sigma;", "\u03C3" },
            { "&nu;", "\u03BD" },
            { "&mu;", "\u03BC" },
            { "&psi;", "\u03C8" },
            { " ", "" },
        };

        static Dictionary<string, string> latexReplacements = new Dictionary<string, string>()
        {
            { "&minus;", "-" },
            { "&prime;", "'" },
            { "cos", "\\cos" },
            { "cosh", "\\cosh" },
            { "tan", "\\tan" },
            { "&", "\\" },
            { ";", "" },
        };

        public static (string strForm, string latexForm, string htmlForm) GetEquationForms(HtmlNode aNode)
        {
            var strFormSpan = aNode.SelectSingleNode("span[@class = 'math']");

            StringBuilder strFormSb = new StringBuilder();

            ProcessNode(strFormSpan, ref strFormSb);
            ApplyStringReplacements(strFormSb);

            string strForm = strFormSb.ToString();
            string latexForm = CreateLatexForm(strForm);
            ClearHtmlFromClasses(strFormSpan);
            string htmlForm = strFormSpan.InnerHtml;

            return (strForm, latexForm, htmlForm);
        }

        static void ProcessNode(HtmlNode strFormSpan, ref StringBuilder strForm)
        {
            foreach (var child in strFormSpan.ChildNodes)
            {
                if (child.Name == "sup")
                    ProcessIndex(child, ref strForm, '^');
                else if (child.Name == "sub")
                    ProcessIndex(child, ref strForm, '_');
                else if (child.HasChildNodes)
                    ProcessNode(child, ref strForm);
                else
                {
                    strForm.Append(child.InnerText.Trim());
                }
            }
        }

        static void ProcessIndex(HtmlNode node, ref StringBuilder strForm, char index)
        {
            strForm.Append(index);
            if (node.HasChildNodes)
            {
                strForm.Append("(");
                ProcessNode(node, ref strForm);
                strForm.Append(")");
            }
        }

        static string CreateLatexForm(string strForm)
        {
            StringBuilder latexFormSb = new StringBuilder(strForm);
            ProcessLatexIndexes(latexFormSb);
            ApplyLatexReplacements(latexFormSb);
            var latexForm = latexFormSb
                .Insert(0, @"\[")
                .Remove(latexFormSb.Length - 1, 1)
                .Append(@"\]").ToString();
            latexForm = ProcessLatexDerivative(latexForm);
            latexForm = ProcessLatexFrac(latexForm);
            return latexForm;
        }

        static void ApplyStringReplacements(StringBuilder strForm)
        {
            foreach (var r in strReplacements)
            {
                strForm = strForm.Replace(r.Key, r.Value);
            }
        }

        static void ProcessLatexIndexes(StringBuilder latexForm)
        {
            bool isIndex = false;
            int openBracetsCount = 0;
            for (int i = 0; i < latexForm.Length; i++)
            {
                if (latexForm[i] == '^' || latexForm[i] == '_')
                {
                    isIndex = true;
                    continue;
                }
                if (latexForm[i] == '(')
                {
                    if (isIndex)
                    {
                        openBracetsCount++;
                        if (openBracetsCount == 1)
                        {
                            latexForm[i] = '{';
                            continue;
                        }
                    }
                }
                if (latexForm[i] == ')')
                {
                    if (isIndex)
                    {
                        openBracetsCount--;
                        if (isIndex && openBracetsCount == 0)
                        {
                            latexForm[i] = '}';
                            isIndex = false;
                        }
                    }
                    
                }
            }
        }

        static void ApplyLatexReplacements(StringBuilder latexForm)
        {
            foreach (var r in latexReplacements)
            {
                latexForm = latexForm.Replace(r.Key, r.Value);
            }
        }

        static string ProcessLatexDerivative(string latexForm)
        {
            Regex regex = new Regex(@"(\w{1}'+\(.*\))|(\w{1}'+)");
            MatchCollection matches = regex.Matches(latexForm.ToString());
            foreach (Match m in matches)
            {
                var derivativeOrder = m.Value.Count(c => c == '\'');
                var element = m.Value.Replace("\'", "");
                var pattern = derivativeOrder > 1 
                    ? $@"\frac{{d^{derivativeOrder}{element}}}{{dx^{derivativeOrder}}}" 
                    : $@"\frac{{d{element}}}{{dx}}";
                latexForm = regex.Replace(latexForm, pattern, 1);
            }
            return latexForm;
        }

        static string ProcessLatexFrac(string latexForm)
        {
            Regex regex = new Regex(@"(\([^(].+\)/\(.+[^)]\))|(\w+/\w+)");
            MatchCollection matches = regex.Matches(latexForm.ToString());
            foreach (Match m in matches)
            {
                var fracElements = m.Value.Split('/');
                var pattern = $@"\frac{{{fracElements[0]}}}{{{fracElements[1]}}}";
                latexForm = regex.Replace(latexForm, pattern, 1);
            }
            return latexForm;
        }

        static void ClearHtmlFromClasses(HtmlNode formula)
        {
            formula.RemoveClass();
            foreach (var node in formula.ChildNodes)
                ClearHtmlFromClasses(node);
        }
    }
}
