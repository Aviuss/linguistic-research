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
    public static Persistance.FourPhrasesFromChapterOne? fourPhrasesFromChapterOne;
    public static Persistance.CacheDB? cacheDB;
    public static ConcurrentDictionary<int, string>? mapIdbToName = null;
    public static List<Persistance.LanguageRules>? listOfLanguageRules = null;

    public static bool doParallelIfPossible = false;
    public static bool showProgressBar = false;


    public static List<Process> runningProcesses = new();
    public static CancellationTokenSource cts = new();
    public static bool dontCreateDataInTemporaryFolder = false;

    public static IpaDistanceProvider? ipaLetterDistanceDict;

    static void Main(string[] args)
    {
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Stopping all processes...");
            e.Cancel = true;
            cts.Cancel();
            KillAllProcesses();
        };

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            KillAllProcesses();
        };

        Console.WriteLine("Welcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!");

        sadownikdb = new Persistance.Sadownikdb(
            dbPath: Path.Combine(dataAndResultsPath, "book database/SadownikDB.sqlite")
        );
        fourPhrasesFromChapterOne = new Persistance.FourPhrasesFromChapterOne(
            Path.Combine(dataAndResultsPath, "book database", "custom_data.json")
        );
        mapIdbToName = Persistance.MapIdbToName.ReadFromFile();
        listOfLanguageRules = Persistance.GetLanguageRules.ReadFromFile();

        cacheDB = new Persistance.CacheDB(
            dbPath: Path.Combine(dataAndResultsPath, "cache/cache.sqlite")
        );

        ipaLetterDistanceDict = new Persistance.IpaDistanceProvider(Path.Combine(dataAndResultsPath, "json settings", "ipa_letter_distance.csv"));

        if (args.Length == 4)
        {
            string jobId = args[0].Trim();
            List<int> bookIdbs = args[1].Split(",").Select(el => int.Parse(el)).ToList();
            List<int> chapters = args[2].Split(",").Select(el => int.Parse(el)).ToList();
            dontCreateDataInTemporaryFolder = args[3] == "true";

            var jobFactory = new JobPresents.JobFactory();
            IJobPreset job = jobFactory.Create(jobId);
            job.bookIDBs = bookIdbs;
            job.chapters = chapters;
            job.getChapterConstruct = sadownikdb;
            job.Start();
        }

        if (args.Length == 0)
        {
            doParallelIfPossible = true;
            showProgressBar = true;

            List<int> pgwary = new List<int>() { 27, 29, 36, 38, 46, 37, 44, 39, 43, 33, 42 };
            pgwary.Sort();


            var jobFactory = new JobPresents.JobFactory();
            IJobPreset job = jobFactory.Create("StandardLevenshteinPreset");
            job.bookIDBs = pgwary;
            job.chapters = new() /*{ -10 };*/ /*{ 1 };*/ { 1 , 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27 };
            job.getChapterConstruct = sadownikdb;
            job.Start();
        }



        cacheDB.Dispose();
        sadownikdb.Dispose();
        Console.WriteLine("Program finished running... .. .");
    }

    static void KillAllProcesses()
    {
        lock (runningProcesses)
        {
            foreach (var p in runningProcesses)
            {
                try
                {
                    if (!p.HasExited)
                        p.Kill(entireProcessTree: true);
                }
                catch { /* ignore */ }
            }
            runningProcesses.Clear();
        }
    }

}