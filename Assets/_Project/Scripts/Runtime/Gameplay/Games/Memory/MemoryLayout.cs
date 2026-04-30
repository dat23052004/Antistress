using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MemoryLayout", menuName = "Memory/Layout")]
public class MemoryLayout : ScriptableObject
{
    [Tooltip("Mỗi string = 1 hàng. '1' = có thẻ, '0' hoặc '_' = trống.")]
    public string[] rows;
    public float cellSize = 90f;
    public float spacing = 12f;

    public List<Vector2Int> GetPositions()
    {
        var result = new List<Vector2Int>();
        if (rows == null) return result;

        for (int r = 0; r < rows.Length; r++)
        {
            string row = rows[r];
            if (string.IsNullOrEmpty(row)) continue;
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] == '1')
                    result.Add(new Vector2Int(c, r));
            }
        }
        return result;
    }

    public bool IsValid()
    {
        int count = GetPositions().Count;
        return count > 0 && count % 2 == 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        int count = GetPositions().Count;
        if (count > 0 && count % 2 != 0)
            Debug.LogWarning($"[MemoryLayout] '{name}': {count} thẻ — phải là số chẵn để tạo cặp.");
    }
#endif
}
