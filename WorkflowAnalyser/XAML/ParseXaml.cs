using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace WorkflowAnalyser.XAML
{
    internal static class Xaml
    {
        public static XmlDocument XmlDocument { get; set; }
        public static XmlNamespaceManager NamespaceManager { get; set; }

       public static void ParseWorkflowFile(string XAMLContents)
        {
            XmlDocument = new XmlDocument();
            NamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);
            XmlDocument.LoadXml(XAMLContents);
            NamespaceManager.AddNamespace("xaml", "http://schemas.microsoft.com/netfx/2009/xaml/activities");
            NamespaceManager.AddNamespace("ui", "http://schemas.uipath.com/workflow/activities");
            NamespaceManager.AddNamespace("sap2010", "http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation");
            foreach (XmlAttribute attribute in XmlDocument.DocumentElement.Attributes)
            {
                if (attribute.Name.StartsWith("xmlns:"))
                {
                    NamespaceManager.AddNamespace(attribute.Name.Substring(attribute.Name.IndexOf(":") + 1), attribute.Value);
                }

            }
        }
        public static string GetInternalPath(XmlNode node)
        {
            List<string> path = new List<string>();
            node = node.ParentNode;
            while(node.LocalName != "Activity" && node.LocalName != "Transition.To")
            {
                if (node.LocalName != "FlowStep" && node.Attributes["sap2010:WorkflowViewState.IdRef"] !=null)
                {
                    if (node.Attributes["DisplayName"] == null)
                        path.Add(node.LocalName);
                    else
                        path.Add(node.Attributes["DisplayName"].Value);
                }
                node = node.ParentNode;

            }
            if (path.Count > 0)
                return String.Join(" > ", path.Reverse<string>());
            else
                return "";
        }
        public static bool IsVariableUsed(string variableName, string stringValue)
        {

            string checkRegex = @".*\W+" + variableName + @"\W+.*";
            if (stringValue != null && stringValue.StartsWith("["))
            {
                Regex.Replace(stringValue, "\".*?\"", "", RegexOptions.Compiled| RegexOptions.IgnoreCase);
                var regex = new Regex(checkRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                Match match = regex.Match(variableName);
                return match.Success;
            }
            return false;

        }
    }
}
