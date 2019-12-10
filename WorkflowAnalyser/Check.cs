using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using WorkflowAnalyser.XAML;

namespace WorkflowAnalyser
{
    internal class Checks
    {
        public void Delay(string strWorkflow, DataTable dtDelay)
        {
            dtDelay.Columns.Add("Path");
            dtDelay.Columns.Add("NodeName");
            dtDelay.Columns.Add("Value");

            Xaml.ParseWorkflowFile(strWorkflow);
            string xPathExpression = "//xaml:Delay | //*[@DelayMS != '{x:Null}' or @DelayBefore != '{x:Null}']";
            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes(xPathExpression, Xaml.NamespaceManager))
            {

                string targetName = "";
                if (node.Attributes["DisplayName"] == null)
                    targetName = node.LocalName;
                else
                    targetName = node.Attributes["DisplayName"].Value;
                if (node.Name.Equals("Delay"))
                {
                    dtDelay.Rows.Add(Xaml.GetInternalPath(node), targetName, node.Attributes["Duration"].Value);
                }
                if (node.Attributes["DelayBefore"] != null)
                {
                    dtDelay.Rows.Add(Xaml.GetInternalPath(node), targetName, node.Attributes["DelayBefore"].Value);
                }
                if (node.Attributes["DelayMS"] != null)
                {
                    dtDelay.Rows.Add(Xaml.GetInternalPath(node), targetName, node.Attributes["DelayMS"].Value);
                }

            }
        }
        public void VariableNammingConvension(string strWorkflow, DataTable dtVariable)
        {
            dtVariable.Columns.Add("Path");
            dtVariable.Columns.Add("Variable Name");


            Xaml.ParseWorkflowFile(strWorkflow);
            string xPathExpression = "//xaml:Variable";
            string variableNammingPattern = "(^(dt_)*([A-Z][a-z0-9]*)+$)";
            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes(xPathExpression, Xaml.NamespaceManager))
            {
                var regex = new Regex(variableNammingPattern);
                Match match = regex.Match(node.Attributes["Name"].Value);
                if (!match.Success)
                {
                    dtVariable.Rows.Add(Xaml.GetInternalPath(node), node.Attributes["Name"].Value);
                }

            }
        }

        public void ArgumentNammingConvension(string strWorkflow, DataTable dtArgument)
        {
            dtArgument.Columns.Add("Path");
            dtArgument.Columns.Add("Argument Name");

            Xaml.ParseWorkflowFile(strWorkflow);
            string xPathExpression = "//x:Property";
            string variableNammingPattern = "(^(in_|out_|io_)(dt_)*([A-Z][a-z0-9]*)+)";
            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes(xPathExpression, Xaml.NamespaceManager))
            {
                var regex = new Regex(variableNammingPattern);
                Match match = regex.Match(node.Attributes["Name"].Value);
                if (!match.Success)
                {
                    dtArgument.Rows.Add("", node.Attributes["Name"].Value);
                }

            }
        }

        public void VariableOverridesArgument(string strWorkflow, DataTable dtVarArg)
        {
            dtVarArg.Columns.Add("Path");
            dtVarArg.Columns.Add("Variable Name");

            Xaml.ParseWorkflowFile(strWorkflow);


            List<string> argumentNames = new List<string>();
            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes("//*/x:Property/@Name", Xaml.NamespaceManager))
                argumentNames.Add(node.Value);

            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes("//*/xaml:Variable", Xaml.NamespaceManager))
            {
                string variableName = node.Attributes["Name"].Value;
                if (argumentNames.Contains(variableName, StringComparer.OrdinalIgnoreCase))
                {
                    dtVarArg.Rows.Add(Xaml.GetInternalPath(node), variableName);
                }
            }


        }

        public void VariableOverridesVariable(string strWorkflow, DataTable dtVarVar)
        {
            dtVarVar.Columns.Add("Path");
            dtVarVar.Columns.Add("Variable Name");

            Xaml.ParseWorkflowFile(strWorkflow);


            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes("//*/xaml:Variable", Xaml.NamespaceManager))
            {
                string variableName = node.Attributes["Name"].Value;
                XmlNode cNode = node.ParentNode.ParentNode;
                foreach (XmlNode dNode in cNode.SelectNodes(".//*/*/xaml:Variable", Xaml.NamespaceManager))
                {
                    string dName = dNode.Attributes["Name"].Value;
                    if (dName.Equals(variableName, StringComparison.OrdinalIgnoreCase))
                        dtVarVar.Rows.Add(Xaml.GetInternalPath(node), variableName);
                }
            }
        }

        public void RepeatedActivityName(string strWorkflow, DataTable dtRepeatedActivitiesName)
        {
            dtRepeatedActivitiesName.Columns.Add("Path");
            dtRepeatedActivitiesName.Columns.Add("Variable Name");

            int threshold = 1;

            Xaml.ParseWorkflowFile(strWorkflow);

            Dictionary<string, List<XmlNode>> dicActivityname = new Dictionary<string, List<XmlNode>>();
            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes("//*[@sap2010:WorkflowViewState.IdRef]", Xaml.NamespaceManager))
            {

                if (node.LocalName.Equals("FlowStep") || node.LocalName.Equals("Catch"))
                    continue;
                string displayName = string.Empty;
                if (node.Attributes["DisplayName"] != null)
                    displayName = node.Attributes["DisplayName"].Value;
                else
                    displayName = node.LocalName;



                if (dicActivityname.ContainsKey(displayName) && node.LocalName != "Sequence")
                    dicActivityname[displayName].Add(node);
                else
                    dicActivityname[displayName] = new List<XmlNode>() { node };



            }

            foreach (string disName in dicActivityname.Keys)
            {
                if (dicActivityname[disName].Count > threshold)
                {
                    foreach (XmlNode xmlNode in dicActivityname[disName])
                    {
                        string targetName = "";
                        if (xmlNode.Attributes["DisplayName"] == null)
                            targetName = xmlNode.LocalName;
                        else
                            targetName = xmlNode.Attributes["DisplayName"].Value;


                        dtRepeatedActivitiesName.Rows.Add(Xaml.GetInternalPath(xmlNode), targetName);
                    }
                }
            }
        }

        public void EmptyCatch(string strWorkflow, DataTable dtEmptyCatch)
        {
            dtEmptyCatch.Columns.Add("Path");
            dtEmptyCatch.Columns.Add("Variable Name");

            Xaml.ParseWorkflowFile(strWorkflow);


            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes("//xaml:Catch[count(xaml:ActivityAction/*)=1]", Xaml.NamespaceManager))
            {
                string displayName = string.Empty;
                if (node.Attributes["DisplayName"] != null)
                    displayName = node.Attributes["DisplayName"].Value;
                else
                    displayName = node.LocalName;
                dtEmptyCatch.Rows.Add(Xaml.GetInternalPath(node), displayName);

            }
        }


        public void DeeplyNested(string strWorkflow, DataTable dtNested)
        {
            dtNested.Columns.Add("Path");
            dtNested.Columns.Add("Variable Name");
            dtNested.Columns.Add("Depth");
            int threshold = 10;
            Xaml.ParseWorkflowFile(strWorkflow);


            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes("//*[@sap2010:WorkflowViewState.IdRef]", Xaml.NamespaceManager))
            {
                XmlNode activityNode = node;
                int depth = 0;
                while (activityNode.LocalName != "Activity" && activityNode.LocalName != "State")
                {
                    if (activityNode.LocalName != "FlowStep" && activityNode.Attributes["sap2010:WorkflowViewState.IdRef"] != null)
                        depth++;
                    activityNode = activityNode.ParentNode;
                }

                if (depth > threshold)
                {
                    Console.WriteLine(depth);
                    string displayName = string.Empty;
                    if (node.Attributes["DisplayName"] != null)
                        displayName = node.Attributes["DisplayName"].Value;
                    else
                        displayName = node.LocalName;

                    dtNested.Rows.Add(Xaml.GetInternalPath(node), displayName, "Code has " + depth.ToString() + " nested activities");
                }


            }
        }


        public void UnusedVariable(string strWorkflow, DataTable dtEmptyCatch)
        {
            dtEmptyCatch.Columns.Add("Path");
            dtEmptyCatch.Columns.Add("Variable Name");

            Xaml.ParseWorkflowFile(strWorkflow);

            foreach (XmlNode node in Xaml.XmlDocument.DocumentElement.SelectNodes("//*[xaml:Sequence.Variables] | //*[xaml:Flowchart.Variables] | //*[xaml:StateMachine.Variables]", Xaml.NamespaceManager))
            {
                foreach (XmlNode item in node.SelectNodes("./*/xaml:Variable", Xaml.NamespaceManager))
                    if (UnusedVariables_IsVariableUsedInContainer(item.Attributes["Name"].Value, node))
                        dtEmptyCatch.Rows.Add(Xaml.GetInternalPath(node), item.Attributes["Name"]);
            }

        }
        private bool UnusedVariables_IsVariableUsedInContainer(string variableName, XmlNode containerNode)
        {
            bool variableUsed = false;
            foreach (XmlNode node in containerNode.SelectNodes(".//*[not(local-name()='Reference') and not(local-name()='DebugSymbol.Symbol') and (string-length(text()) > 0)]"))
                if (Xaml.IsVariableUsed(variableName, node.InnerText))
                    break;

            if (!variableUsed)
                foreach (XmlNode item in containerNode.SelectNodes(".//@*"))
                    if (Xaml.IsVariableUsed(variableName, item.Value))
                        break;


            return variableUsed;
        }
    }
}
