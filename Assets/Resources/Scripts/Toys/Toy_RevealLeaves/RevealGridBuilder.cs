using UnityEngine;

internal sealed class RevealGridBuilder
{
    internal readonly struct LayoutSettings
    {
        public LayoutSettings(
            float spawnSpacingX,
            float spawnSpacingY,
            bool staggerOddRows,
            float rowOffsetX,
            float overscanX,
            float overscanY)
        {
            SpawnSpacingX = spawnSpacingX;
            SpawnSpacingY = spawnSpacingY;
            StaggerOddRows = staggerOddRows;
            RowOffsetX = rowOffsetX;
            OverscanX = overscanX;
            OverscanY = overscanY;
        }

        public float SpawnSpacingX { get; }
        public float SpawnSpacingY { get; }
        public bool StaggerOddRows { get; }
        public float RowOffsetX { get; }
        public float OverscanX { get; }
        public float OverscanY { get; }
    }

    public Transform ResolveLeafRoot(Transform ownerTransform, Transform currentLeafRoot)
    {
        if (currentLeafRoot != null)
            return currentLeafRoot;

        Transform existingRoot = ownerTransform.Find("LeafRoot");
        if (existingRoot != null)
            return existingRoot;

        GameObject rootObject = new GameObject("LeafRoot");
        Transform leafRoot = rootObject.transform;
        leafRoot.SetParent(ownerTransform, false);
        return leafRoot;
    }

    public bool RebuildGrid(
        SpriteRenderer backgroundRenderer,
        Transform leafRoot,
        GameObject leafPrefab,
        LayoutSettings layout,
        Object context)
    {
        if (leafRoot == null)
        {
            Debug.LogWarning("RevealController requires a leafRoot transform.", context);
            return false;
        }

        ClearSpawnedLeaves(leafRoot);

        if (!CanBuildLeafGrid(backgroundRenderer, leafPrefab, layout, context))
            return false;

        SpawnLeafGrid(backgroundRenderer, leafRoot, leafPrefab, layout);
        return true;
    }

    private static bool CanBuildLeafGrid(
        SpriteRenderer backgroundRenderer,
        GameObject leafPrefab,
        LayoutSettings layout,
        Object context)
    {
        if (backgroundRenderer == null)
        {
            Debug.LogWarning("RevealController requires a background SpriteRenderer.", context);
            return false;
        }

        if (leafPrefab == null)
        {
            Debug.LogWarning("RevealController requires a leafPrefab to spawn the reveal grid.", context);
            return false;
        }

        if (layout.SpawnSpacingX <= 0f || layout.SpawnSpacingY <= 0f)
        {
            Debug.LogWarning("RevealController requires positive spawn spacing values.", context);
            return false;
        }

        if (leafPrefab.GetComponentInChildren<SpriteRenderer>(true) == null ||
            leafPrefab.GetComponentInChildren<Collider2D>(true) == null)
        {
            Debug.LogWarning("leafPrefab must contain at least one SpriteRenderer and one Collider2D.", context);
            return false;
        }

        return true;
    }

    private static void ClearSpawnedLeaves(Transform leafRoot)
    {
        while (leafRoot.childCount > 0)
        {
            Transform child = leafRoot.GetChild(0);
            if (Application.isPlaying)
            {
                child.SetParent(null, false);
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static void SpawnLeafGrid(
        SpriteRenderer backgroundRenderer,
        Transform leafRoot,
        GameObject leafPrefab,
        LayoutSettings layout)
    {
        Bounds bounds = backgroundRenderer.bounds;
        float minX = bounds.min.x - layout.OverscanX;
        float maxX = bounds.max.x + layout.OverscanX;
        float minY = bounds.min.y - layout.OverscanY;
        float maxY = bounds.max.y + layout.OverscanY;

        int rowIndex = 0;
        for (float y = minY; y <= maxY + 0.001f; y += layout.SpawnSpacingY)
        {
            float xOffset = layout.StaggerOddRows && (rowIndex & 1) == 1 ? layout.RowOffsetX : 0f;

            for (float x = minX; x <= maxX + 0.001f; x += layout.SpawnSpacingX)
                SpawnLeafInstance(leafRoot, leafPrefab, new Vector2(x + xOffset, y));

            rowIndex++;
        }
    }

    private static void SpawnLeafInstance(Transform leafRoot, GameObject leafPrefab, Vector2 worldPosition)
    {
        GameObject leafInstance = Object.Instantiate(leafPrefab, leafRoot, false);
        leafInstance.name = leafPrefab.name;

        Transform instanceTransform = leafInstance.transform;
        Vector3 localPosition = instanceTransform.localPosition;
        Vector3 localSpawnPosition = leafRoot.InverseTransformPoint(
            new Vector3(worldPosition.x, worldPosition.y, leafRoot.position.z));

        localPosition.x = localSpawnPosition.x;
        localPosition.y = localSpawnPosition.y;
        instanceTransform.localPosition = localPosition;
    }
}
