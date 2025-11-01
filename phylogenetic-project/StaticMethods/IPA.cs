using System;
using System.Text;

namespace phylogenetic_project.StaticMethods;

public class IPA
{
    public static List<string[]> ConvertToIpa(string text, Persistance.LanguageRules rule)
    {
        List<StringBuilder[]> choices = [];
        int maxCharLen = 0;
        foreach (var element in rule.Rules)
        {
            maxCharLen = Math.Max(maxCharLen, element.Key.Length);
        }

        int index = 0;
        while (index < text.Length)
        {
            List<string> charList = [];
            for (int i_append = 0; i_append < maxCharLen && index + i_append < text.Length; i_append++)
            {
                charList.Add("");
                for (int i = 0; i < i_append + 1; i++)
                {
                    charList[charList.Count - 1] += text[index + i];
                }
            }

            string[]? addChar = null;
            for (int ci = charList.Count - 1; ci >= 0; ci--)
            {
                string character = charList[ci];
                rule.Rules.TryGetValue(character, out string[]? keyValue);
                if (keyValue != null)
                {
                    addChar = keyValue;
                    index += ci + 1;
                    break;
                }
            }

            if (addChar == null)
            {
                addChar = new string[] { text[index].ToString() };
                index += 1;
            }



            if (addChar.Length == 1 && choices.Count > 0 && choices[choices.Count - 1].Length == 1)
            {
                choices[choices.Count - 1][0].Append(addChar[0]);
            }
            else
            {
                choices.Add(addChar.Select(s => new StringBuilder(s)).ToArray());
            }


        }

        return choices.Select(element =>
        {
            return element.Select(el => el.ToString()).ToArray();
        }).ToList();
    }
    
    public static (List<string?>, List<string[]>) ExtractChoiceVector(List<string[]> ipaText)
    {
        List<string?> textBlankChoice = [];
        List<string[]> choices = [];

        for (int i = 0; i < ipaText.Count; i++)
        {
            if (ipaText[i].Length == 1)
            {
                textBlankChoice.Add(ipaText[i][0]);
            } else
            {
                textBlankChoice.Add(null);
                choices.Add(ipaText[i]);
            }
        }

        return (textBlankChoice, choices);
    }
}
