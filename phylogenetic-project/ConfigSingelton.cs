using System;
using ArgsParser;
using phylogenetic_project.JobPresents;
using phylogenetic_project.Persistance;

namespace phylogenetic_project;

public sealed class ConfigSingelton
{
    public string[] args = [];
    public string outputFolderPath = null!;
    public bool noPython = false;
    public IGetChapter? inputStruct = null;
    public IJobPreset? jobPreset = null;
    public List<int>? bookIdbs = null;
    public List<int>? chapters = null;
    
    private string? job = null;
    private string? inputTypePath = null;
    private string? inputType = null;

    private static ConfigSingelton instance = null!;
    private static object creationLock = new();
    private static object passArgsLock = new();
    private List<IDisposable> disposables = [];
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
                .RequiresOption<string>("output-folder-path", "Path of Program's output")
                .RequiresOption<string>("book-idbs", "Idb's of books to analyze from input")
                .RequiresOption<string>("chapters", "Chapters of books to analyze from input")
                .SupportsFlag("no-python", "Disables python scripts")
                .Parse();
            
            if (parser.HasErrors)
            {
                parser.ShowErrors(indent, "Issues:");
                throw new Exception("Error in provided args.");
            }

            instance.job = parser.GetOption<string>("job");
            instance.inputType = parser.GetOption<string>("input-type");
            instance.inputTypePath = parser.GetOption<string>("input-type-path");
            instance.outputFolderPath = parser.GetOption<string>("output-folder-path");
            ArgumentNullException.ThrowIfNull(instance.outputFolderPath);
            Directory.CreateDirectory(instance.outputFolderPath);

            instance.bookIdbs = parser.GetOption<string>("book-idbs").Split(",").Select(x => Int32.Parse(x)).ToList();
            instance.chapters = parser.GetOption<string>("chapters").Split(",").Select(x => Int32.Parse(x)).ToList();
            instance.noPython = !parser.IsFlagProvided("no-python");

            instance.LoadInputType();
            instance.LoadJobPreset();
        }
    }

    private void LoadInputType()
    {
        ArgumentNullException.ThrowIfNull(this.inputType);
        ArgumentNullException.ThrowIfNull(this.inputTypePath);

        if (this.inputType == "sql")
        {
            Sadownikdb sadownik = new(
                dbPath: this.inputTypePath
            );

            this.disposables.Add(sadownik);
            this.inputStruct = sadownik;
            return;
        } else if (this.inputType == "json") {
            this.inputStruct = new Persistance.FourPhrasesFromChapterOne(
                this.inputTypePath
            );
            return;
        }

        throw new Exception("wrong inputType type. Can be only \"sql\" or \"json\" ");
    }

    private void LoadJobPreset()
    {
        ArgumentNullException.ThrowIfNull(this.inputStruct);
        ArgumentNullException.ThrowIfNull(this.chapters);
        ArgumentNullException.ThrowIfNull(this.bookIdbs);

        string timeNow = DateTime.UtcNow.ToString("yyyy.MM.dd_HHmmss");
        
        if (job == "phylogenetic-tree-standard-text")
        {
            this.jobPreset = new phylogenetic_project.JobPresets.Collection.StandardLevenshtein(
                this.inputStruct,
                this.chapters,
                this.bookIdbs,
                Path.Combine(this.outputFolderPath, "results", "phylogenetic-tree-standard-text", timeNow),
                this.noPython
            );
        }

    }

    public void Dispose()
    {
        foreach (IDisposable item in this.disposables)
        {
            item.Dispose();
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