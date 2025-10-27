using System;
using System.IO;
namespace phylogenetic_project;

public class Program
{
    public static readonly string dataAndResultsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\data and results");

    public static Persistance.Sadownikdb sadownikdb = null!;


    void RequireNonNullProperties()
    {
        ArgumentNullException.ThrowIfNull(sadownikdb);
    }

    public Program()
    {
        sadownikdb = new Persistance.Sadownikdb(
            dbPath: Path.Combine(dataAndResultsPath, "book database/SadownikDB.sqlite")
        );

        RequireNonNullProperties();
    }

    static void Main()
    {
        Console.WriteLine("Welcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!");
        _ = new Program();
        Console.WriteLine("Program finished running... .. .");
    }

}
