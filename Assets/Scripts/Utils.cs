using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public static class Utils
{
    public static List<T> Shuffle<T>(IEnumerable<T> samples)
    {
        List<T> shuffled = samples.ToList();

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            T temp = shuffled[j];
            shuffled[j] = shuffled[i];
            shuffled[i] = temp;
        }

        return shuffled;
    }
    public static bool ExistPointInRect(Vector3 point, RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        return corners[0].x < point.x && point.x < corners[2].x &&
               corners[0].y < point.y && point.y < corners[2].y;
    }
    public static void TMPDOText(TextMeshProUGUI text, float duration)
    {
        text.maxVisibleCharacters = 0;
        DOTween.To(x => text.maxVisibleCharacters = (int)x, 0f, text.text.Length, duration);
    }
}
