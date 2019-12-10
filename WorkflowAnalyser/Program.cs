using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowAnalyser.XAML;

namespace WorkflowAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {

            string strWorkflow = File.ReadAllText(@"C:\Users\Bharat\Documents\UiPath\BlankProcess27\Main.xaml");
            DataTable dt = new DataTable();
            Checks checks = new Checks();
            checks.Delay(strWorkflow,dt);

            dt = new DataTable();
            checks.VariableNammingConvension(strWorkflow, dt);

            dt = new DataTable();
            checks.ArgumentNammingConvension(strWorkflow, dt);

            dt = new DataTable();
            checks.VariableOverridesArgument(strWorkflow, dt);

            dt = new DataTable();
            checks.RepeatedActivityName(strWorkflow, dt);

            dt = new DataTable();
            checks.EmptyCatch(strWorkflow, dt);


            dt = new DataTable();
            checks.DeeplyNested(strWorkflow, dt);

            dt = new DataTable();
            checks.UnusedVariable(strWorkflow, dt);

        }
    }
}
