using UnityEngine;

[CreateAssetMenu(fileName = "MemoryLayout", menuName = "Memory/Layout")]
public class MemoryLayout : ScriptableObject
{
    [System.Serializable]
    public class RowData
    {
        public int count = 4;
        [Tooltip("Khoảng cách ngang giữa các thẻ trong hàng.")]
        public float xSpacing = 12f;
        [Tooltip("Dịch ngang tính theo đơn vị cell. 0 = canh giữa, 0.5 = xen kẽ nửa ô.")]
        public float xOffset = 0f;
    }

    public RowData[] rows;
    [Tooltip("Khoảng cách dọc giữa các hàng.")]
    public float ySpacing = 12f;
    [Tooltip("Kích thước mỗi thẻ (width = height).")]
    public float cardSize = 90f;

    public int TotalCards
    {
        get
        {
            int total = 0;
            if (rows != null) foreach (var r in rows) total += r.count;
            return total;
        }
    }

    public bool IsValid() => TotalCards > 0 && TotalCards % 2 == 0;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (TotalCards % 2 != 0)
            Debug.LogWarning($"[MemoryLayout] '{name}': tổng {TotalCards} thẻ — phải là số chẵn.");
    }
#endif
}
