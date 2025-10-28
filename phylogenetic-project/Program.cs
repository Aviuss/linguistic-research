using phylogenetic_project.JobPresents;
using System;
using System.IO;
namespace phylogenetic_project;

public class Program
{
    public static readonly string dataAndResultsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\data and results");

    public static Persistance.Sadownikdb? sadownikdb;


    static void Main()
    {
        Console.WriteLine("Welcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!");

        sadownikdb = new Persistance.Sadownikdb(
            dbPath: Path.Combine(dataAndResultsPath, "book database/SadownikDB.sqlite")
        );


        var jobFactory = new JobPresents.JobFactory();

        IJobPreset job =  jobFactory.Create("StandardLevenshteinAlgorithm");
        job.bookIDBs = new List<int>() { 27, 44, 32, 28 };
        job.chapters = sadownikdb.Chapters;
        job.getChapterConstruct = sadownikdb;
        job.Start();

        

        sadownikdb.Dispose();
        Console.WriteLine("Program finished running... .. .");
    } 

}