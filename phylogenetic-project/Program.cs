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
                --input-type json --input-type-path ../../../../input_data/chapter_16_sentence_maching.json --input-type-id chapter_16_sentence_maching
                --output-folder-path ../../../../output_data/
                --book-idbs 44,46
                --chapters 2
                --ipa-rules ../../../../input_data/ipa_rules.json --ipa-rules-id ipa_rules_1
                --custom-ipa-distance ../../../../input_data/ipa_letter_distance.csv --custom-ipa-distance-id custom-ipa-distance-1
                --map-idb-to-name ../../../../input_data/map_idb_to_name.json
                --random-ipa-iterations 10
                ".Split(" ").Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        }

        config.PassArgs(args);

        RegisterShutdownHandlers();
        //LoadDataAndConfigAndCache();

        Console.WriteLine("\n\nWelcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!\n");

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
