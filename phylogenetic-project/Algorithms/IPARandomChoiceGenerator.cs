using System;
using System.Text;

namespace phylogenetic_project.Algorithms;

public class IPARandomChoiceGenerator
{
    public static IEnumerable<(string, string)> ReturnRandomChoice
        (List<string[]> inputText1, List<string[]> inputText2, long randomSize = 10000)
    {
        List<string?> textBlankChoice1, textBlankChoice2;
        List<string[]> choiceForText1, choiceForText2;

        (textBlankChoice1, choiceForText1) = StaticMethods.IPA.ExtractChoiceVector(inputText1);
        (textBlankChoice2, choiceForText2) = StaticMethods.IPA.ExtractChoiceVector(inputText2);

        List<string[]> choicesVector = choiceForText1.Concat(choiceForText2).ToList();

        long amountOfTotalCombinations = 1;
        for (int i = 0; i < choicesVector.Count; i++)
        {
            amountOfTotalCombinations *= choicesVector[i].Length;
            if (amountOfTotalCombinations > randomSize)
            {
                amountOfTotalCombinations = long.MaxValue;
                break;
            }
        }

        IEnumerable<ReadOnlyMemory<string>>? cartesian = null;

        if (amountOfTotalCombinations <= randomSize)
        {
            cartesian = CartesianProductSpan(choicesVector);
        }
        else
        {
            cartesian = CartesianProductSpanRandomChoice(choicesVector, randomSize);
        }


        foreach (var comboMem in cartesian)
        {
            string[] combination = comboMem.Span.ToArray();
            StringBuilder txt1 = new StringBuilder("");
            StringBuilder txt2 = new StringBuilder("");
            int idx_comb = 0;


            foreach ((StringBuilder, List<string?>) item
                in new (StringBuilder, List<string?>)[] { (txt1, textBlankChoice1), (txt2, textBlankChoice2) })
            {
                foreach (string? element in item.Item2)
                {
                    if (element == null)
                    {
                        item.Item1.Append(combination[idx_comb]);
                        idx_comb++;
                    }
                    else
                    {
                        item.Item1.Append(element);
                    }
                }
            }

            yield return (txt1.ToString(), txt2.ToString());
        }
    }

    private static IEnumerable<ReadOnlyMemory<T>> CartesianProductSpan<T>(IReadOnlyList<IReadOnlyList<T>> sequences)
    {
        int[] indices = new int[sequences.Count];
        T[] current = new T[sequences.Count];

        while (true)
        {
            for (int i = 0; i < sequences.Count; i++)
                current[i] = sequences[i][indices[i]];

            yield return current.AsMemory(); // same buffer reused

            int pos = sequences.Count - 1;
            while (pos >= 0)
            {
                indices[pos]++;
                if (indices[pos] < sequences[pos].Count)
                    break;
                indices[pos] = 0;
                pos--;
            }
            if (pos < 0)
                yield break;
        }
    }

    private static IEnumerable<ReadOnlyMemory<T>> CartesianProductSpanRandomChoice<T>(IReadOnlyList<IReadOnlyList<T>> sequences, long iterations)
    {
        var rand = Random.Shared;
        T[] current = new T[sequences.Count];

        for (int n = 0; n < iterations; n++)
        {
            for (int i = 0; i < sequences.Count; i++)
                current[i] = sequences[i][rand.Next(sequences[i].Count)];

            yield return current.AsMemory();
        }
    }


}
