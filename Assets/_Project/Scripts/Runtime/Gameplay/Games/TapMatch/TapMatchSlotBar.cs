using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapMatchSlotBar : MonoBehaviour
{
    [SerializeField] private TapMatchSlot[] slots;

    private TapMatchGame game;

    public bool IsFull => CountOccupied() >= slots.Length;

    public void Init(TapMatchGame ownerGame)
    {
        game = ownerGame;
        foreach (var slot in slots)
            slot.Clear();
    }

    public IEnumerator AddTile(TapMatchTile tile)
    {
        int emptyIndex = FindFirstEmpty();
        if (emptyIndex < 0) yield break;

        // Tile bay đến vị trí slot → tự destroy khi đến nơi
        Vector3 slotWorldPos = slots[emptyIndex].RectTransform.position;
        yield return tile.AnimateToSlot(slotWorldPos);

        // Slot hiển thị icon sau khi tile đến nơi
        slots[emptyIndex].Occupy(tile.TileType, tile.Icon);
        slots[emptyIndex].PunchScale();

        SortSlots();
        yield return CheckMatch();
    }

    private void SortSlots()
    {
        List<(int type, Sprite icon)> occupied = new List<(int, Sprite)>();

        foreach (var slot in slots)
        {
            if (slot.IsOccupied)
                occupied.Add((slot.TileType, slot.GetIcon()));
        }

        occupied.Sort((a, b) => a.type.CompareTo(b.type));

        foreach (var slot in slots)
            slot.Clear();

        for (int i = 0; i < occupied.Count; i++)
            slots[i].Occupy(occupied[i].type, occupied[i].icon);
    }

    private IEnumerator CheckMatch()
    {
        for (int i = 0; i <= slots.Length - 3; i++)
        {
            if (slots[i].IsOccupied &&
                slots[i + 1].IsOccupied &&
                slots[i + 2].IsOccupied &&
                slots[i].TileType == slots[i + 1].TileType &&
                slots[i + 1].TileType == slots[i + 2].TileType)
            {
                yield return PlayMatchAndClear(i);
                yield break;
            }
        }

        if (IsFull)
            game.OnBarFull();
    }

    private IEnumerator PlayMatchAndClear(int startIndex)
    {
        AudioManager.Ins.PlaySfx(SfxCue.Tile_Solve);

        bool done0 = false, done1 = false, done2 = false;
        slots[startIndex].PlayMatchAnimation(() => done0 = true);
        slots[startIndex + 1].PlayMatchAnimation(() => done1 = true);
        slots[startIndex + 2].PlayMatchAnimation(() => done2 = true);

        yield return new WaitUntil(() => done0 && done1 && done2);

        SortSlots();
        game.OnMatchCleared();
    }

    private int FindFirstEmpty()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].IsOccupied) return i;
        return -1;
    }

    private int CountOccupied()
    {
        int count = 0;
        foreach (var slot in slots)
            if (slot.IsOccupied) count++;
        return count;
    }
}
