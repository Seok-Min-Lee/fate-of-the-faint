using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public static class Utils
{
    public static List<T> Shuffle<T>(IEnumerable<T> samples)
    {
        List<T> shuffled = samples.ToList();

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            T temp = shuffled[j];
            shuffled[j] = shuffled[i];
            shuffled[i] = temp;
        }

        return shuffled;
    }
    public static void TMPDOText(TextMeshProUGUI text, float duration)
    {
        text.maxVisibleCharacters = 0;
        DOTween.To(x => text.maxVisibleCharacters = (int)x, 0f, text.text.Length, duration);
    }
}
