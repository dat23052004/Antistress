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

    private readonly List<MemoryCard> _activeCards = new List<MemoryCard>();

    public int TotalPairs { get; private set; }

    public IEnumerator SpawnBoard(MemoryGame game, MemoryLayout layout)
    {
        ClearBoard();

        if (layout == null || !layout.IsValid())
        {
            Debug.LogError("[MemoryBoard] Layout null hoặc không hợp lệ.");
            yield break;
        }

        TotalPairs = layout.TotalCards / 2;

        float cardSize = layout.cardSize;
        float yStep    = cardSize + layout.ySpacing;
        int   rowCount = layout.rows.Length;

        List<Sprite> icons     = PickRandomIcons(TotalPairs);
        List<int>    cardTypes = BuildCardTypes(TotalPairs);
        Shuffle(cardTypes);

        float gridH  = rowCount * cardSize + (rowCount - 1) * layout.ySpacing;
        float originY = gridH / 2f - cardSize / 2f;

        int cardIndex = 0;
        for (int r = 0; r < rowCount; r++)
        {
            MemoryLayout.RowData row = layout.rows[r];
            float xStep  = cardSize + row.xSpacing;
            float rowW   = row.count * cardSize + (row.count - 1) * row.xSpacing;
            float originX = -rowW / 2f + cardSize / 2f + row.xOffset * xStep;

            for (int c = 0; c < row.count; c++)
            {
                Vector2 pos = new(originX + c * xStep, originY - r * yStep);

                GameObject go = Instantiate(_cardPrefab, _cardContainer);
                go.GetComponent<RectTransform>().anchoredPosition = pos;

                MemoryCard card = go.GetComponent<MemoryCard>();
                card.Init(cardTypes[cardIndex], icons[cardTypes[cardIndex]], game, cardSize);
                _activeCards.Add(card);
                cardIndex++;

                if (cardIndex % 4 == 0) yield return null;
            }
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
}
