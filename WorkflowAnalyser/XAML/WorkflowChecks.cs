﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowAnalyser.XAML
{

    class Core
    {

        public void WorkflowCheck(string projectPath, DataTable dtIssues)
        {
            string strWorkflow = File.ReadAllText(projectPath);
        }
    }
}
