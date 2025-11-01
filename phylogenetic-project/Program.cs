using phylogenetic_project.JobPresents;
using phylogenetic_project.Persistance;
using phylogenetic_project.StaticMethods;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace phylogenetic_project;

public class Program
{
    public static readonly string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..");
    public static readonly string dataAndResultsPath = Path.Combine(projectPath, @"data and results");
    
    public static Persistance.Sadownikdb? sadownikdb;

    public static ConcurrentDictionary<int, string>? mapIdbToName = null;
    public static List<Persistance.LanguageRules>? listOfLanguageRules = null;

    static void Main()
    {
        Console.WriteLine("Welcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!");

        sadownikdb = new Persistance.Sadownikdb(
            dbPath: Path.Combine(dataAndResultsPath, "book database/SadownikDB.sqlite")
        );
        mapIdbToName = Persistance.MapIdbToName.ReadFromFile();
        listOfLanguageRules = Persistance.GetLanguageRules.ReadFromFile();


        
        List<int> pgwary = new List<int>() { 27, 29, 36, 38, 46, 37, 44, 39, 43, 33, 42 };
        pgwary.Sort();


        var jobFactory = new JobPresents.JobFactory();
        IJobPreset job = jobFactory.Create("IPARandomChoiceLevenshteinAlgorithm");
        job.bookIDBs = pgwary;
        job.chapters = new() { 1 };
        job.getChapterConstruct = sadownikdb;
        job.Start();


        sadownikdb.Dispose();
        Console.WriteLine("Program finished running... .. .");
    }
    

}