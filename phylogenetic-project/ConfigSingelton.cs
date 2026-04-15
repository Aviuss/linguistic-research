using System;
using ArgsParser;
using phylogenetic_project.JobPresents;

namespace phylogenetic_project;

public sealed class ConfigSingelton
{
    public string[] args = [];

    private string? job = null;


    private static ConfigSingelton instance = null!;
    private static object creationLock = new object();
    private static object passArgsLock = new object();

    private ConfigSingelton() {}

    public void PassArgs(string[] args)
    {
        lock (passArgsLock)
        {
            instance.args = args;

            var indent = 2;
            var parser = new Parser(args)
                .RequiresOption<string>("job", "Job which script needs to perform")
                .Parse();
            
            if (parser.HasErrors)
            {
                parser.ShowErrors(indent, "Issues:");
                return;
            }

            instance.job = parser.GetOption<string>("job");
        }
    }

    public IJobPreset? GetJobPreset()
    {
        ArgumentNullException.ThrowIfNull(job);

        if (job.StartsWith("phylogenetic-tree-standard-text"))
        {
            return new phylogenetic_project.JobPresets.Collection.StandardLevenshtein(
                null!,
                null!,
                null!
            );
        }

        return null;
    }

    public static ConfigSingelton Instance
    {
        get
        {
            lock (creationLock)
            {
                if (instance == null)
                {
                    instance = new ConfigSingelton();
                }
                return instance;
            }
        }
    }
}