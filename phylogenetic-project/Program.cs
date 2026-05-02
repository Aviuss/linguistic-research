using phylogenetic_project.JobPresets;
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
    public static List<Process> runningProcesses = new();
    public static CancellationTokenSource cts = new();
    public static ConfigSingelton config = ConfigSingelton.Instance;

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"Current Working Directory: \"{Directory.GetCurrentDirectory()}\"");
            Console.WriteLine("DEVELOPMENT ARGS INJECTION");
            args = @"
                --job phylogenetic-tree-ipa-random-choice
                
                --input-type sql
                --input-type-path ../../../../input_data/SadownikDB.sqlite
                --input-type-id sadownikdb

                --output-folder-path ../../../../output_data/

                --book-idbs 29,28,44,43,39,38,37,46,36,33,42
                --chapters 1
                --map-idb-to-name ../../../../input_data/map_idb_to_name.json
                --ipa-rules ../../../../input_data/ipa_rules.json
                --no-python
                --random-ipa-iterations 5
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
