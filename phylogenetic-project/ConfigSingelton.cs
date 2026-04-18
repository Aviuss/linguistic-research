using System;
using ArgsParser;
using phylogenetic_project.JobPresents;
using phylogenetic_project.Persistance;

namespace phylogenetic_project;

public sealed class ConfigSingelton
{
    public string[] args = [];
    public IGetChapter? inputStruct = null;
    public IJobPreset? jobPreset = null;

    private string? job = null;
    private string? inputTypePath = null;
    private string? inputType = null;

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
                .RequiresOption<string>("input-type", "Type of input for IGetChapter")
                .RequiresOption<string>("input-type-path", "Path of input for IGetChapter")
                .Parse();
            
            if (parser.HasErrors)
            {
                parser.ShowErrors(indent, "Issues:");
                throw new Exception("Error in provided args.");
            }

            instance.job = parser.GetOption<string>("job");
            instance.inputType = parser.GetOption<string>("input-type");
            instance.inputTypePath = parser.GetOption<string>("input-type-path");
            
            instance.loadInputType();
            instance.loadJobPreset();
        }
    }

    private void loadInputType()
    {
        ArgumentNullException.ThrowIfNull(this.inputType);
        ArgumentNullException.ThrowIfNull(this.inputTypePath);

        if (this.inputType == "sql")
        {
            this.inputStruct = new Persistance.Sadownikdb(
                dbPath: this.inputTypePath
            );
            return;
        } else if (this.inputType == "json") {
            this.inputStruct = new Persistance.FourPhrasesFromChapterOne(
                this.inputTypePath
            );
            return;
        }

        throw new Exception("wrong inputType type. Can be only \"sql\" or \"json\" ");
    }

    private void loadJobPreset()
    {
        ArgumentNullException.ThrowIfNull(this.inputStruct);


        if (job == "phylogenetic-tree-standard-text")
        {
            this.jobPreset = new phylogenetic_project.JobPresets.Collection.StandardLevenshtein(
                this.inputStruct,
                null!,
                null!
            );
        }

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