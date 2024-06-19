using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EqGraph
{
    public static class ParserService
    {
        const string baseUrl = "https://eqworld.ipmnet.ru";

        public static List<Equation> ParseSite()
        {
            var urls = GetUrls();
            List<Equation> result = new List<Equation>();
            var htmlLoader = InitializeHtmlLoader();

            foreach (string url in urls)
            {
                var htmlDocument = htmlLoader.Load(url);
                var htmlTag = htmlDocument.DocumentNode.SelectSingleNode("html");
                string baseType = htmlTag.SelectSingleNode("//head/meta[@name='Description']")
                    .GetAttributeValue("content", "");

                var equationsContainer = htmlTag.SelectSingleNode($"//div[@class='fixedwidth']");
                ClearNodeFrom(equationsContainer, new string[] { "\r", "\n", "&nbsp;", "  " });
                var ols = equationsContainer.SelectNodes("./ol");

                foreach (var ol in ols)
                {
                    var types = GetTypesList(baseType, ol);
                    var lis = ol.SelectNodes("li");

                    foreach (var li in lis)
                    {
                        var anchor = li.SelectSingleNode("a");

                        var pdfLink = GetPdfLink(anchor);
                        var (stringForm, latexForm, htmlForm) = EquationFormsBuilder.GetEquationForms(anchor);
                        var label = GetLabel(li, anchor);

                        Equation equationToAdd = new Equation(types, pdfLink, label, stringForm, latexForm, htmlForm);
                        result.Add(equationToAdd);
                    }
                }
            }
            return result;
        }

        static List<string> GetUrls()
        {
            Console.WriteLine("Чтение URL-адресов...");
            string path = "urls.txt";
            var urls = new List<string>();
            using (StreamReader reader = new StreamReader(path))
            {
                string url;
                while ((url = reader.ReadLine()) != null)
                {
                    urls.Add(url);
                }
            }
            return urls;
        }

        static HtmlWeb InitializeHtmlLoader()
        {
            HtmlWeb htmlLoader = new HtmlWeb();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            htmlLoader.OverrideEncoding = Encoding.GetEncoding("windows-1251");
            return htmlLoader;
        }

        static void ClearNodeFrom(HtmlNode node, IEnumerable<string> toClear)
        {
            foreach (var element in toClear)
            {
                node.InnerHtml = node.InnerHtml.Replace(element, "");
            }
        }

        static List<string> GetTypesList(string baseType, HtmlNode olNode)
        {
            var result = new List<string> { baseType };
            var subType = olNode.PreviousSibling;
            if (subType.Name == "h3")
            {
                StringBuilder sb = new StringBuilder();
                foreach (var child in subType.ChildNodes)
                {
                    if (child.Name == "span")
                        continue;
                    else
                        sb.Append(child.InnerText);
                }
                sb.Remove(0, 5);
                result.Add(sb.ToString());
            }
            return result;
        }

        static string GetPdfLink(HtmlNode aNode)
        {
            var pdfLink = aNode.GetAttributeValue("href", "").Replace("../../..", "").Insert(0, baseUrl);
            return pdfLink;
        }

        static string GetLabel(HtmlNode liNode, HtmlNode aNode)
        {
            string label = "";
            if (liNode.ChildNodes.Count != 1)
            {
                var labelTag = liNode.SelectSingleNode("i");
                if (labelTag == null)
                    labelTag = aNode.NextSibling;
                label = labelTag.InnerText.Replace(".", "").Trim();
            }
            label = !string.IsNullOrEmpty(label) ? label : "Без названия";
            return label;
        }
    }
}
