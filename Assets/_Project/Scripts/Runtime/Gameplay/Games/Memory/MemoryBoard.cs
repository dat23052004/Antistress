using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MemoryBoard : MonoBehaviour
{
    [Header("Card Settings")]
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private RectTransform _cardContainer;
    [SerializeField] private List<Sprite> _cardIcons;

    [Header("Layouts")]
    [SerializeField] private List<MemoryLayout> _layouts;

    private readonly List<MemoryCard> _activeCards = new List<MemoryCard>();

    public int TotalPairs { get; private set; }

    public IEnumerator SpawnBoard(MemoryGame game)
    {
        ClearBoard();

        MemoryLayout layout = PickLayout();
        if (layout == null || !layout.IsValid())
        {
            Debug.LogError("[MemoryBoard] Không có layout hợp lệ.");
            yield break;
        }

        List<Vector2Int> positions = layout.GetPositions();
        int pairCount = positions.Count / 2;
        TotalPairs = pairCount;

        List<Sprite> icons = PickRandomIcons(pairCount);
        List<int> cardTypes = BuildCardTypes(pairCount);
        Shuffle(cardTypes);

        float step = layout.cellSize + layout.spacing;
        Vector2 centerOffset = ComputeCenterOffset(positions, step);

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];
            int type = cardTypes[i];

            GameObject go = Instantiate(_cardPrefab, _cardContainer);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = GetPixelPosition(pos, step) + centerOffset;

            MemoryCard card = go.GetComponent<MemoryCard>();
            card.Init(type, icons[type], game);
            _activeCards.Add(card);

            if (i % 4 == 0) yield return null;
        }
    }

    private void ClearBoard()
    {
        foreach (var card in _activeCards)
        {
            if (card != null)
            {
                card.transform.DOKill();
                Destroy(card.gameObject);
            }
        }
        _activeCards.Clear();
    }

    private MemoryLayout PickLayout()
    {
        if (_layouts == null || _layouts.Count == 0) return null;
        return _layouts[Random.Range(0, _layouts.Count)];
    }

    private List<Sprite> PickRandomIcons(int count)
    {
        var pool = new List<Sprite>(_cardIcons);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.GetRange(0, Mathf.Min(count, pool.Count));
    }

    private List<int> BuildCardTypes(int pairCount)
    {
        var types = new List<int>(pairCount * 2);
        for (int t = 0; t < pairCount; t++)
        {
            types.Add(t);
            types.Add(t);
        }
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

    private Vector2 ComputeCenterOffset(List<Vector2Int> positions, float step)
    {
        int minCol = int.MaxValue, maxCol = int.MinValue;
        int minRow = int.MaxValue, maxRow = int.MinValue;

        foreach (var p in positions)
        {
            if (p.x < minCol) minCol = p.x;
            if (p.x > maxCol) maxCol = p.x;
            if (p.y < minRow) minRow = p.y;
            if (p.y > maxRow) maxRow = p.y;
        }

        float centerCol = (minCol + maxCol) / 2f;
        float centerRow = (minRow + maxRow) / 2f;
        return new Vector2(-centerCol * step, centerRow * step);
    }

    private Vector2 GetPixelPosition(Vector2Int pos, float step)
    {
        return new Vector2(pos.x * step, -pos.y * step);
    }
}
