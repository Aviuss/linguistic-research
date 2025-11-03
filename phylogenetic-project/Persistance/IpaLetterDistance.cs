using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
namespace phylogenetic_project.Persistance;

public class IpaDistanceProvider
{
    private ConcurrentDictionary<(string, string), decimal> ipaLetterDistanceDict;

    public IpaDistanceProvider(string path)
    {   
        ipaLetterDistanceDict = ReadPhoneticCsv(path);
    }

    public decimal this[string a, string b]
    {
        get
        {
            if (a == b)
                return 0;

            var key = string.CompareOrdinal(a, b) < 0 ? (a, b) : (b, a);
            return ipaLetterDistanceDict.TryGetValue(key, out var value)
                ? value
                : 1;
        }
    }

    public static ConcurrentDictionary<(string, string), decimal> ReadPhoneticCsv(string path)
    {
        var dict = new ConcurrentDictionary<(string, string), decimal>();
        var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        using var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        string[]? headers = parser.ReadFields();
        if (headers == null) return dict;

        while (!parser.EndOfData)
        {
            string[]? fields = parser.ReadFields();
            if (fields == null || fields.Length == 0) continue;

            string rowSymbol = fields[0].Trim();
            if (string.IsNullOrEmpty(rowSymbol)) continue;

            for (int j = 1; j < fields.Length && j < headers.Length; j++)
            {
                string colSymbol = headers[j].Trim();
                string? rawValue = fields[j]?.Trim().Trim('"');

                if (string.IsNullOrEmpty(rawValue))
                    continue;

                string normalized = rawValue.Replace(',', '.');

                if (decimal.TryParse(normalized, NumberStyles.Any, numberFormat, out decimal value))
                {
                    if (string.CompareOrdinal(rowSymbol, colSymbol) < 0)
                    {
                        dict.TryAdd((rowSymbol, colSymbol), value);
                    }
                    else
                    {
                        dict.TryAdd((colSymbol, rowSymbol), value);
                    }

                }
            }
        }

        return dict;
    }

}


