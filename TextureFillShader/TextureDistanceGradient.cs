using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[ExecuteInEditMode]
public class TextureDistanceGradient : MonoBehaviour
{
    public enum NeighborMode { FourWay, EightWay }

    [Header("Input / Output")]
    public Texture2D InputTexture;
    public bool ApplyOnStart = true;

    [Header("Gradient Options")]
    public Gradient DistanceGradient = new Gradient();

    [Header("Distance Settings")]
    public NeighborMode Neighbors = NeighborMode.EightWay;
    public bool PreserveAlpha = true;
    public bool RespectAlphaFlow = true;

    [Header("Speed Mapping (Green Channel)")]
    [Tooltip("Minimum speed multiplier for green = 0")]
    public float MinSpeed = 1f;
    [Tooltip("Maximum speed multiplier for green = 1")]
    public float MaxSpeed = 5f;

#if UNITY_EDITOR
    [Header("Asset Saving")]
    public bool SaveAsAsset = true;
    public string SaveFolder = "GeneratedTextures";
    public string SaveFileName = "DistanceGradientTexture";
#endif

    void Start()
    {
        if (Application.isPlaying && ApplyOnStart && InputTexture != null)
            ApplyAndDisplay();
    }

    [ContextMenu("Apply Distance Gradient")]
    public void ApplyAndDisplay()
    {
        if (InputTexture == null)
        {
            Debug.LogWarning("No input texture assigned.");
            return;
        }

        Texture2D output = ApplyDistanceGradient(InputTexture, Neighbors);
        var rend = GetComponent<Renderer>();
        if (rend && rend.sharedMaterial != null)
            rend.sharedMaterial.mainTexture = output;

#if UNITY_EDITOR
        if (SaveAsAsset)
            SaveTextureAsAsset(output);
#endif
    }

    public Texture2D ApplyDistanceGradient(Texture2D tex, NeighborMode mode)
    {
        int width = tex.width;
        int height = tex.height;
        Color[] pixels = tex.GetPixels();
        float[,] distances = new float[width, height];

        // Initialize
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                distances[x, y] = float.MaxValue;

        // Priority queue
        var queue = new SortedSet<(float dist, Vector2Int pos)>(
            Comparer<(float, Vector2Int)>.Create((a, b) =>
                a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : a.Item2.GetHashCode().CompareTo(b.Item2.GetHashCode()))
        );

        // Find red sources
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = pixels[y * width + x];
                if (c.a > 0f && c.r > 0.9f)
                {
                    distances[x, y] = 0f;
                    queue.Add((0f, new Vector2Int(x, y)));
                }
            }
        }

        // Directions
        List<(Vector2Int offset, float cost)> directions = new List<(Vector2Int, float)>
        {
            (new Vector2Int(1, 0), 1f),
            (new Vector2Int(-1, 0), 1f),
            (new Vector2Int(0, 1), 1f),
            (new Vector2Int(0, -1), 1f)
        };
        if (mode == NeighborMode.EightWay)
        {
            directions.Add((new Vector2Int(1, 1), 1.41421356f));
            directions.Add((new Vector2Int(-1, 1), 1.41421356f));
            directions.Add((new Vector2Int(1, -1), 1.41421356f));
            directions.Add((new Vector2Int(-1, -1), 1.41421356f));
        }

        // Propagate with alpha and speed logic
        while (queue.Count > 0)
        {
            var current = queue.Min;
            queue.Remove(current);

            float dist = current.dist;
            Vector2Int p = current.pos;
            if (dist > distances[p.x, p.y])
                continue;

            float srcAlpha = pixels[p.y * width + p.x].a;

            foreach (var (dir, cost) in directions)
            {
                int nx = p.x + dir.x;
                int ny = p.y + dir.y;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                int idx = ny * width + nx;
                Color target = pixels[idx];

                if (target.a <= 0f)
                    continue;

                // Respect alpha flow (only downhill)
                if (RespectAlphaFlow && target.a > srcAlpha)
                    continue;

                // Green channel controls speed (map 0–1 → MinSpeed–MaxSpeed)
                float green = Mathf.Clamp01(target.g);
                float speed = Mathf.Lerp(MinSpeed, MaxSpeed, green);
                float timeCost = cost / speed; // faster speed = lower time cost

                float newDist = dist + timeCost;
                if (newDist < distances[nx, ny])
                {
                    distances[nx, ny] = newDist;
                    queue.Add((newDist, new Vector2Int(nx, ny)));
                }
            }
        }

        // Normalize distances
        float maxDist = 0f;
        foreach (float d in distances)
            if (d < float.MaxValue && d > maxDist)
                maxDist = d;

        // Build result texture
        Color[] newPixels = new Color[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                if (distances[x, y] == float.MaxValue)
                {
                    newPixels[idx] = new Color(0, 0, 0, PreserveAlpha ? pixels[idx].a : 0);
                }
                else
                {
                    float t = distances[x, y] / maxDist;
                    Color col = DistanceGradient.Evaluate(t);
                    if (PreserveAlpha)
                        col.a = pixels[idx].a;
                    newPixels[idx] = col;
                }
            }
        }

        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.SetPixels(newPixels);
        result.Apply();
        return result;
    }

#if UNITY_EDITOR
    private void SaveTextureAsAsset(Texture2D texture)
    {
        string folderPath = Path.Combine("Assets", SaveFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(folderPath, $"{SaveFileName}_{timestamp}.png");

        File.WriteAllBytes(filePath, texture.EncodeToPNG());
        AssetDatabase.Refresh();

        if (AssetImporter.GetAtPath(filePath) is TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log($"✅ Saved generated texture to: {filePath}");
    }
#endif
}
