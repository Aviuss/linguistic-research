using System;
using System.IO;
namespace phylogenetic_project;

public class Program
{
    public static readonly string dataAndResultsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\data and results");

    public static Persistance.Sadownikdb? sadownikdb;


    static void Main()
    {
        Console.WriteLine("Welcome to phylogenetic project, where research paper is being created in the Poznan University of Technology!");

        sadownikdb = new Persistance.Sadownikdb(
            dbPath: Path.Combine(dataAndResultsPath, "book database/SadownikDB.sqlite")
        );

        var levenshteinMatrix = new Matrices.BookMatrix<Matrices.CellChapterJobs.LevenshteinCellChapterJob_FieldData>(
            bookIDBs_: new List<int>() { 27, 44, 32, 28},
            chapters_: sadownikdb.Chapters,
            matrixCellChapterJob_: new Matrices.CellChapterJobs.LevenshteinCellChapterJob(sadownikdb)
        );

        var x = levenshteinMatrix.GetResultMatrix();
        Console.WriteLine(levenshteinMatrix.ToString());

        sadownikdb.Dispose();
        Console.WriteLine("Program finished running... .. .");
    }

}
