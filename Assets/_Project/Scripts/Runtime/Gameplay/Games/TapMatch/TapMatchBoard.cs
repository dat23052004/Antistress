using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TilePosition
{
    public float col, row;
    public int layer;
    public TilePosition(float col, float row, int layer)
    {
        this.col = col; this.row = row; this.layer = layer;
    }
}

public class TapMatchBoard : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private RectTransform tileContainer;
    [SerializeField] private List<Sprite> tileIcons;

    [Header("Layouts")]
    [SerializeField] private List<TapMatchLayout> layouts;
    [SerializeField] private float cellSize = 90f;
    [SerializeField] private float spacing = 8f;

    private class TileNode
    {
        public TapMatchTile tile;
        public TilePosition pos;
    }

    private TapMatchGame game;
    private readonly List<TileNode> activeNodes = new List<TileNode>();
    private bool accepting = true;

    public bool HasTiles => activeNodes.Count > 0;

    public void Init(TapMatchGame ownerGame)
    {
        game = ownerGame;
    }

    public IEnumerator SpawnBoard()
    {
        ClearBoard();
        accepting = true;

        TapMatchLayout layout = PickLayout();
        if (layout == null || !layout.IsValid())
        {
            Debug.LogError("TapMatchBoard: no valid layout assigned.");
            yield break;
        }

        TilePosition[] positions = layout.GetPositions();
        int typeCount = Mathf.Min(layout.typeCount, tileIcons.Count);
        int setsPerType = layout.setsPerType;
        List<Sprite> selectedIcons = PickRandomIcons(typeCount);
        List<int> tileTypes = GenerateTileTypes(typeCount, setsPerType);
        Shuffle(tileTypes);

        // Sort by layer ascending: thấp trước, cao sau → cao render đè lên (SetAsLastSibling)
        List<TilePosition> sorted = new List<TilePosition>(positions);
        sorted.Sort((a, b) => a.layer.CompareTo(b.layer));

        Vector2 centerOffset = ComputeCenterOffset(positions);

        for (int i = 0; i < sorted.Count; i++)
        {
            TilePosition pos = sorted[i];
            int type = tileTypes[i % tileTypes.Count];

            GameObject go = Instantiate(tilePrefab, tileContainer);
            go.GetComponent<RectTransform>().anchoredPosition = GetPixelPosition(pos) + centerOffset;
            go.transform.SetAsLastSibling();

            TapMatchTile tile = go.GetComponent<TapMatchTile>();
            tile.Init(type, selectedIcons[type], this);

            activeNodes.Add(new TileNode { tile = tile, pos = pos });

            if (i % 5 == 0) yield return null;
        }

        UpdateAllBlockStates();
    }

    public void OnTileClicked(TapMatchTile tile)
    {
        if (!accepting) return;

        TileNode node = activeNodes.Find(n => n.tile == tile);
        if (node == null || node.tile.IsBlocked) return;

        AudioManager.Ins.PlaySfx(SfxCue.Tile_Click);
        activeNodes.Remove(node);
        accepting = false;

        UpdateAllBlockStates();
        StartCoroutine(SendTileToBar(tile));
    }

    private IEnumerator SendTileToBar(TapMatchTile tile)
    {
        yield return game.SlotBar.AddTile(tile);
        accepting = true;

        if (!HasTiles)
            game.OnBoardCleared();
    }

    private void UpdateAllBlockStates()
    {
        foreach (var node in activeNodes)
            node.tile.SetBlocked(IsBlockedBy(node));
    }

    private bool IsBlockedBy(TileNode target)
    {
        foreach (var other in activeNodes)
        {
            if (other == target) continue;
            if (other.pos.layer > target.pos.layer
                && Mathf.Abs(other.pos.col - target.pos.col) < 1f
                && Mathf.Abs(other.pos.row - target.pos.row) < 1f)
                return true;
        }
        return false;
    }

    private TapMatchLayout PickLayout()
    {
        if (layouts == null || layouts.Count == 0) return null;
        return layouts[Random.Range(0, layouts.Count)];
    }

    private void ClearBoard()
    {
        foreach (var node in activeNodes)
        {
            if (node.tile != null)
                Destroy(node.tile.gameObject);
        }
        activeNodes.Clear();
    }

    private List<Sprite> PickRandomIcons(int count)
    {
        List<Sprite> pool = new List<Sprite>(tileIcons);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.GetRange(0, Mathf.Min(count, pool.Count));
    }

    private List<int> GenerateTileTypes(int typeCount, int setsPerType)
    {
        List<int> types = new List<int>();
        for (int t = 0; t < typeCount; t++)
            for (int s = 0; s < setsPerType * 3; s++)
                types.Add(t);
        return types;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private Vector2 ComputeCenterOffset(TilePosition[] positions)
    {
        float minCol = float.MaxValue, maxCol = float.MinValue;
        float minRow = float.MaxValue, maxRow = float.MinValue;

        foreach (var p in positions)
        {
            if (p.col < minCol) minCol = p.col;
            if (p.col > maxCol) maxCol = p.col;
            if (p.row < minRow) minRow = p.row;
            if (p.row > maxRow) maxRow = p.row;
        }

        float step = cellSize + spacing;
        float centerCol = (minCol + maxCol) / 2f;
        float centerRow = (minRow + maxRow) / 2f;
        return new Vector2(-centerCol * step, centerRow * step);
    }

    private Vector2 GetPixelPosition(TilePosition pos)
    {
        float step = cellSize + spacing;
        return new Vector2(pos.col * step, -pos.row * step);
    }
}
