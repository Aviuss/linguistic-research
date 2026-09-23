using System;
using System.Globalization;
using phylogenetic_project.Persistance;

namespace phylogenetic_project.StaticMethods;


public static class VerboseLog
{
    public const int NormalCombinationLimit = 5;
    public const int NormalCropLength = 200;

    public static bool IsEnabled => writer != null;
    public static bool IsHigh => level == "high";

    private static string level = null!;
    private static IDictionary<int, string>? mapIdbToName = null;
    private static StreamWriter? writer = null;
    private static readonly object writeLock = new();

    public static void Configure(string? level_, IDictionary<int, string>? mapIdbToName_)
    {
        ArgumentNullException.ThrowIfNull(level_);
        if (level_ != "normal" && level_ != "high")
        {
            throw new Exception($"Wrong --verbose level \"{level_}\". Can be only \"normal\" or \"high\".");
        }
        
        level = level_;
        mapIdbToName = mapIdbToName_;
    }

    public static IDisposable Open(string folderPath)
    {
        if (level != null)
        {
            Directory.CreateDirectory(folderPath);
            string path = Path.Combine(folderPath, "verbose.log");
            writer = new StreamWriter(path);
            Console.WriteLine($"Verbose log ({level}): \"{path}\"\n");
        }
        return new Closer();
    }

    public static void WriteLine(string text)
    {
        ArgumentNullException.ThrowIfNull(writer);

        lock (writeLock)
        {
            writer.WriteLine(text);
        }
    }

    public static void Section(string title)
    {
        WriteLine($"\n{new string('=', 80)}\n{title}\n{new string('=', 80)}");
    }

    public static string BookLabel(int idb)
    {
        if (mapIdbToName != null && mapIdbToName.TryGetValue(idb, out string? name))
        {
            return $"{idb} ({name})";
        }
        return idb.ToString();
    }

    /// <summary>
    /// Logs every (book, chapter) once: getChapter -> normalization -> IPA conversion.
    /// </summary>
    public static void LogBooks(IGetChapter getChapter, List<int> bookIDBs, List<int> chapters, LanguageRules[]? languageRules = null)
    {
        if (!IsEnabled) { return; }

        Section("BOOKS: getChapter -> normalization -> IPA ([a|b] marks an ambiguous choice)");
        foreach (int idb in bookIDBs)
        {
            LanguageRules? rule = languageRules == null ? null : Array.Find(languageRules, element => element.IdbCompatible.Contains(idb));

            foreach (int chapterNo in chapters)
            {
                WriteLine($"\n--- book {BookLabel(idb)} | chapter {chapterNo} ---");

                string text = getChapter.GetChapter(idb, chapterNo);
                if (getChapter is GetChapterNormalized normalized)
                {
                    WriteLine($"getChapter:\n  {Crop(normalized.inner.GetChapter(idb, chapterNo))}");
                    WriteLine($"normalized:\n  {Crop(text)}");
                }
                else
                {
                    WriteLine($"getChapter:\n  {Crop(text)}");
                }

                if (rule != null)
                {
                    var ipa = IPA.ConvertToIpa(text, rule);
                    WriteLine($"IPA:\n  {Crop(string.Concat(ipa.Select(element => element.Length == 1 ? element[0] : $"[{string.Join("|", element)}]")))}");
                }
            }
        }
    }

    /// <summary>
    /// Logs one evaluated IPA combination of a book pair: every one in high, first few (cropped) in normal. Thread safe.
    /// </summary>
    public static void Combination(long index, string text1, string text2, decimal distance, decimal maxLength)
    {
        if (!IsEnabled || (!IsHigh && index >= NormalCombinationLimit)) { return; }

        WriteLine($"combination #{index + 1}: distance={distance}, maxLength={maxLength}\n  {Crop(text1)}\n  {Crop(text2)}");
    }

    private static string Crop(string text)
    {
        var info = new StringInfo(text);
        if (IsHigh || info.LengthInTextElements <= NormalCropLength) { return text; }

        return $"{info.SubstringByTextElements(0, NormalCropLength)}... (+{info.LengthInTextElements - NormalCropLength} more)";
    }

    private static void Close()
    {
        lock (writeLock)
        {
            writer?.Dispose();
            writer = null;
        }
    }

    private sealed class Closer : IDisposable
    {
        public void Dispose() => Close();
    }
}
