using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.StaticMethods;

public class SaveTemporaryResults
{
    public static void Save(string folderPath, (string, string)[] fileNameAndContent, bool savePrintStatus = false)
    {
        Directory.CreateDirectory(folderPath);
        foreach (var element in fileNameAndContent)
        {
            File.WriteAllText(Path.Combine(folderPath, element.Item1), element.Item2);
        }
        
        if (savePrintStatus)
            Console.WriteLine($"All results saved in: \"{folderPath}\" \n");   
    }
}
