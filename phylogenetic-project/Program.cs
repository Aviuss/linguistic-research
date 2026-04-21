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
    public static readonly string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");
    public static readonly string dataAndResultsPath = Path.Combine(projectPath, @"data and results");

    public static Persistance.Sadownikdb sadownikdb = null!;
    public static Persistance.FourPhrasesFromChapterOne fourPhrasesFromChapterOne = null!;
    public static Persistance.CacheDB? cacheDB;
    public static ConcurrentDictionary<int, string> mapIdbToName = null!;
    public static List<Persistance.LanguageRules> listOfLanguageRules = null!;

    public static bool doParallelIfPossible = false;
    public static bool showProgressBar = false;


    public static List<Process> runningProcesses = new();
    public static CancellationTokenSource cts = new();
    public static bool dontCreateDataInTemporaryFolder = false;

    public static IpaDistanceProvider? ipaLetterDistanceDict;

    public static ConfigSingelton config = ConfigSingelton.Instance;

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"Current Working Directory: \"{Directory.GetCurrentDirectory()}\"");
            Console.WriteLine("DEVELOPMENT ARGS INJECTION");
            args = @"
                --job phylogenetic-tree-standard-text
                
                --input-type sql
                --input-type-path ../../../../input_data/SadownikDB.sqlite

                --output-folder-path ../../../../output_data/

                --book-idbs 28,29,36,38,46,37,44,39,43,33,42
                --chapters 1,2,3
                ".Split(" ").Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        }

        Console.WriteLine(args);
        config.PassArgs(args);

        RegisterShutdownHandlers();
        //LoadDataAndConfigAndCache();

        Console.WriteLine("Welcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!");

        if (config.jobPreset == null)
        {
            Console.WriteLine("Error: could not create given job :(");
            return;
        }

        config.jobPreset.Start();

        config.Dispose();
        Console.WriteLine("Program finished running.");
    }

    static void RegisterShutdownHandlers()
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
    }

    static void LoadDataAndConfigAndCache()
    {        

        mapIdbToName = Persistance.MapIdbToName.ReadFromFile();
        listOfLanguageRules = Persistance.GetLanguageRules.ReadFromFile();

        cacheDB = new Persistance.CacheDB(
            dbPath: Path.Combine(dataAndResultsPath, "cache/cache.sqlite")
        );

        ipaLetterDistanceDict = new Persistance.IpaDistanceProvider(Path.Combine(dataAndResultsPath, "json settings", "ipa_letter_distance.csv"));
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
