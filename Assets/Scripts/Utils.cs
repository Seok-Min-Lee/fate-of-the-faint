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
    public static List<T> PickRandom<T>(IEnumerable<T> source, int count)
    {
        List<T> pool = new List<T>(source);
        List<T> result = new List<T>(count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
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
    public static List<Vector3> GetCircleAlignedPositions(int count, int radius)
    {
        List<Vector3> positions = new List<Vector3>();
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;

            Vector3 position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            positions.Add(position);
        }

        return positions;
    }
}
