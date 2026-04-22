using System;
using System.Collections.Concurrent;
using ArgsParser;
using phylogenetic_project.JobPresents;
using phylogenetic_project.Persistance;

namespace phylogenetic_project;

public sealed class ConfigSingelton
{
    public IJobPreset? jobPreset = null;

    private string[] args = [];
    private string outputFolderPath = null!;
    private bool noPython = false;
    private IGetChapter? inputStruct = null;
    private List<int>? bookIdbs = null;
    private List<int>? chapters = null;
    private string? job = null;
    private string? inputType = null;
    private string? inputTypePath = null;
    private string? inputTypeId = null;
    private string? customIpaDistancePath = null;
    private ConcurrentDictionary<int, string>? mapIdbToName = null;

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
                .RequiresOption<string>("input-type-id", "Resource identifier for IGetChapter")
                .RequiresOption<string>("output-folder-path", "Path of Program's output")
                .RequiresOption<string>("book-idbs", "Idb's of books to analyze from input")
                .RequiresOption<string>("chapters", "Chapters of books to analyze from input")
                .SupportsFlag("no-python", "Disables python scripts")
                .SupportsOption<string>("custom-ipa-distance", "Path to custom ipa distance")
                .SupportsOption<string>("map-idb-to-name", "Path to map to idb json file")
                .Parse();
            
            if (parser.HasErrors)
            {
                parser.ShowErrors(indent, "Issues:");
                throw new Exception("Error in provided args.");
            }

            instance.job = parser.GetOption<string>("job");
            instance.inputType = parser.GetOption<string>("input-type");
            instance.inputTypePath = parser.GetOption<string>("input-type-path");
            instance.inputTypeId = parser.GetOption<string>("input-type-id");
            instance.outputFolderPath = parser.GetOption<string>("output-folder-path");
            ArgumentNullException.ThrowIfNull(instance.outputFolderPath);
            Directory.CreateDirectory(instance.outputFolderPath);

            instance.bookIdbs = parser.GetOption<string>("book-idbs").Split(",").Select(x => Int32.Parse(x)).ToList();
            instance.bookIdbs.Sort();
            instance.chapters = parser.GetOption<string>("chapters").Split(",").Select(x => Int32.Parse(x)).ToList();
            instance.chapters.Sort();
            instance.noPython = !parser.IsFlagProvided("no-python");
            instance.customIpaDistancePath = parser.GetOption<string>("custom-ipa-distance");
            
            string? mapIdbToNameFilePath = parser.GetOption<string>("map-idb-to-name");
            if (mapIdbToNameFilePath != null)
            {
                instance.mapIdbToName = Persistance.MapIdbToName.ReadFromFile(mapIdbToNameFilePath);
            }

            instance.LoadInputType();
            instance.LoadJobPreset();
        }
    }

    private void LoadInputType()
    {
        ArgumentNullException.ThrowIfNull(this.inputType);
        ArgumentNullException.ThrowIfNull(this.inputTypePath);
        ArgumentNullException.ThrowIfNull(this.inputTypeId);

        if (this.inputType == "sql")
        {
            Sadownikdb sqlResource = new(
                dbPath: this.inputTypePath,
                resourceId: this.inputTypeId
            );

            this.disposables.Add(sqlResource);
            this.inputStruct = sqlResource;
            return;
        } else if (this.inputType == "json") {
            this.inputStruct = new Persistance.FourPhrasesFromChapterOne(
                dataPath: this.inputTypePath,
                resourceId: this.inputTypeId
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
        
        switch (job)
        {
            case "phylogenetic-tree-standard-text":
                this.jobPreset = new phylogenetic_project.JobPresets.Collection.StandardLevenshtein(
                    getChapterConstruct: this.inputStruct,
                    chapters: this.chapters,
                    bookIDBs: this.bookIdbs,
                    outputResultPath: Path.Combine(this.outputFolderPath, "results", "phylogenetic-tree-standard-text", timeNow),
                    noPython: this.noPython
                );
                return;

            case "phylogenetic-tree-ipa-singular-choice":
                this.jobPreset = new phylogenetic_project.JobPresets.Collection.IPAFirstSingularChoiceLevenshtein(
                    getChapterConstruct: this.inputStruct,
                    chapters: this.chapters,
                    bookIDBs: this.bookIdbs,
                    outputResultPath: Path.Combine(this.outputFolderPath, "results", "phylogenetic-tree-standard-text", timeNow),
                    noPython: this.noPython
                );

                return;
            case "phylogenetic-tree-ipa-random-choice":
                throw new NotImplementedException(job);

            default:
                throw new Exception("wrong job type. Check documentation for job input.");
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