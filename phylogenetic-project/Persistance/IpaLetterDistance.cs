using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
namespace phylogenetic_project.Persistance;

public class IpaCustomLetterDistance
{
    public string resourceId;
    private ConcurrentDictionary<(string, string), decimal> ipaLetterDistanceDict;

    public IpaCustomLetterDistance(string path, string resourceId)
    {   
        this.ipaLetterDistanceDict = ReadPhoneticCsv(path);
        this.resourceId = resourceId;
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

    public bool HasString(string a)
    {
        foreach (var key in ipaLetterDistanceDict)
        {
            if (a == key.Key.Item1 || a == key.Key.Item2)
            {
                return true;
            }
        }
        return false;
    }

    private static ConcurrentDictionary<(string, string), decimal> ReadPhoneticCsv(string path)
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

            if (new StringInfo(rowSymbol).LengthInTextElements != 1)
            {
                throw new Exception("ipa letter distance element lenght must be 1");
            }

            for (int j = 1; j < fields.Length && j < headers.Length; j++)
            {
                string colSymbol = headers[j].Trim();
                string? rawValue = fields[j]?.Trim().Trim('"');

                if (string.IsNullOrEmpty(rawValue))
                    continue;
                
                if (new StringInfo(colSymbol).LengthInTextElements != 1)
                {
                    throw new Exception("ipa letter distance element lenght must be 1");
                }

                string normalized = rawValue.Replace(',', '.');

                if (decimal.TryParse(normalized, NumberStyles.Any, numberFormat, out decimal value))
                {
                    if (value < 0 || value > 1)
                    {
                        throw new Exception("parsed value should be in range [0; 1] for IPA letter distance");
                    }

                    var key = string.CompareOrdinal(rowSymbol, colSymbol) < 0
                        ? (rowSymbol, colSymbol)
                        : (colSymbol, rowSymbol);

                    if (dict.TryGetValue(key, out decimal existing) && existing != value)
                    {
                        throw new Exception(
                            $"IPA letter distance matrix is not symmetric: [{rowSymbol}; {colSymbol}] => {value}, but [{colSymbol}; {rowSymbol}] => {existing}");
                    }

                    dict.TryAdd(key, value);
                }
                else
                {
                    throw new Exception(
                        $"IPA letter distance value must be a number or empty: [{rowSymbol}; {colSymbol}] => \"{rawValue}\"");
                }
            }
        }

        return dict;
    }

}


