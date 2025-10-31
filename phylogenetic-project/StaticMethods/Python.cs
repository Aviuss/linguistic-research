using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.StaticMethods;
public class Python
{
    public static readonly string pythonScriptsPath = Path.Combine(Program.projectPath, @"python-scripts");


    public static void CallPythonScript(string scriptName, string[]? arguments = null)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python";
        start.ArgumentList.Add(Path.Combine(pythonScriptsPath, scriptName));

        for (int i = 0; arguments != null && i < arguments.Length; i++)
        {
            start.ArgumentList.Add(arguments[i]);
        }

        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;

        using (Process? process = Process.Start(start))
        {
            if (process != null)
            {
                using StreamReader reader = process.StandardOutput;
                string result = reader.ReadToEnd();
                Console.WriteLine(result);
            }
        }

    }
    
}
