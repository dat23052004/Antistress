using System.Collections.Generic;
using UnityEngine;

internal sealed class RevealLeafField
{
    private readonly List<RevealLeaf> leaves = new List<RevealLeaf>();
    private readonly List<RevealLeaf> animatingLeaves = new List<RevealLeaf>();

    private Collider2D[] overlapResults = new Collider2D[32];
    private ContactFilter2D overlapFilter;

    public bool HasLeaves => leaves.Count > 0;

    public void Clear()
    {
        leaves.Clear();
        animatingLeaves.Clear();
        overlapResults = new Collider2D[32];
        overlapFilter = default;
    }

    public void CacheLeaves(Transform leafRoot, LayerMask leafMask)
    {
        Clear();

        if (leafRoot == null)
            return;

        for (int i = 0; i < leafRoot.childCount; i++)
        {
            Transform child = leafRoot.GetChild(i);
            if (!child.TryGetComponent(out RevealLeaf leaf))
                continue;

            if (!leaf.Prepare())
            {
                Debug.LogWarning($"RevealLeaf on '{child.name}' is missing serialized references.", child);
                continue;
            }

            leaves.Add(leaf);
        }

        overlapResults = new Collider2D[Mathf.Max(32, leaves.Count)];
        overlapFilter = new ContactFilter2D();
        overlapFilter.useTriggers = Physics2D.queriesHitTriggers;
        overlapFilter.SetLayerMask(leafMask);
    }

    public void HideAllLeaves()
    {
        animatingLeaves.Clear();

        for (int i = 0; i < leaves.Count; i++)
            leaves[i].HideInstant();
    }

    public void RevealAtPoint(
        Vector2 worldPoint,
        Vector2 swipeDirection,
        float brushRadius,
        float entryOffset,
        float rotationFromSwipe,
        float randomRotationJitter)
    {
        if (brushRadius <= 0f || leaves.Count == 0)
            return;

        int hitCount = Physics2D.OverlapCircle(worldPoint, brushRadius, overlapFilter, overlapResults);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = overlapResults[i];
            overlapResults[i] = null;

            RevealLeaf leaf = FindLeaf(hitCollider);
            if (leaf == null)
                continue;

            if (leaf.Reveal(swipeDirection, entryOffset, rotationFromSwipe, randomRotationJitter))
                animatingLeaves.Add(leaf);
        }
    }

    public void UpdateAnimations(float deltaTime, float revealDuration)
    {
        for (int i = animatingLeaves.Count - 1; i >= 0; i--)
        {
            if (!animatingLeaves[i].TickAnimation(deltaTime, revealDuration))
                animatingLeaves.RemoveAt(i);
        }
    }

    private RevealLeaf FindLeaf(Collider2D collider)
    {
        if (collider == null)
            return null;

        for (int i = 0; i < leaves.Count; i++)
        {
            if (leaves[i].MatchesCollider(collider))
                return leaves[i];
        }

        return null;
    }
}
