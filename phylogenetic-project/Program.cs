using phylogenetic_project.JobPresents;
using phylogenetic_project.StaticMethods;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
namespace phylogenetic_project;

public class Program
{
    public static readonly string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..");
    public static readonly string dataAndResultsPath = Path.Combine(projectPath, @"data and results");
    
    public static Persistance.Sadownikdb? sadownikdb;

    public static ConcurrentDictionary<int, string>? mapIdbToName = null;

    static void Main()
    {
        Console.WriteLine("Welcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!");

        sadownikdb = new Persistance.Sadownikdb(
            dbPath: Path.Combine(dataAndResultsPath, "book database/SadownikDB.sqlite")
        );
        mapIdbToName = Persistance.MapIdbToName.ReadFromFile();

        var jobFactory = new JobPresents.JobFactory();

        IJobPreset job = jobFactory.Create("StandardLevenshteinAlgorithm");
        job.bookIDBs = new List<int>() { 27, 44, 32, 28 };
        job.chapters = new List<int>() { 1, 2, 4, 8, 16 };
        job.getChapterConstruct = sadownikdb;
        job.Start();



        sadownikdb.Dispose();
        Console.WriteLine("Program finished running... .. .");
    } 

}