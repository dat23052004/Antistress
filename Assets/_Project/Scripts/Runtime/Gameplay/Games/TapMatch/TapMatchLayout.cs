using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TapMatchLayout", menuName = "TapMatch/Layout")]
public class TapMatchLayout : ScriptableObject
{
    [System.Serializable]
    public class LayerData
    {
        [Tooltip("Offset col/row so với layer 0. Dùng 0.5 để tile đè lên khe giữa layer dưới.")]
        public Vector2 offset = Vector2.zero;

        [Tooltip("Mỗi string = 1 hàng. '1' = có tile, '0' hoặc '_' = trống.")]
        public string[] rows;
    }

    public LayerData[] layers;

    [Tooltip("Số loại tile khác nhau trong màn này")]
    public int typeCount = 4;
    [Tooltip("Mỗi loại xuất hiện bao nhiêu bộ 3")]
    public int setsPerType = 2;

    public TilePosition[] GetPositions()
    {
        List<TilePosition> result = new List<TilePosition>();

        if (layers == null) return result.ToArray();

        for (int l = 0; l < layers.Length; l++)
        {
            LayerData layer = layers[l];
            if (layer.rows == null) continue;

            for (int r = 0; r < layer.rows.Length; r++)
            {
                string row = layer.rows[r];
                if (string.IsNullOrEmpty(row)) continue;

                for (int c = 0; c < row.Length; c++)
                {
                    if (row[c] == '1')
                        result.Add(new TilePosition(c + layer.offset.x, r + layer.offset.y, l));
                }
            }
        }

        return result.ToArray();
    }

    public int TileCount => GetPositions().Length;

    public bool IsValid()
    {
        int count = TileCount;
        int expected = typeCount * setsPerType * 3;
        return count > 0 && count == expected;
    }
}
