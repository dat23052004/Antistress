using DG.Tweening;
using UnityEngine;

public class SettingPopup : MonoBehaviour
{
    [SerializeField] private RectTransform popupPanel;

    private bool isOpen = false;

    public void TogglePopup()
    {
        isOpen = !isOpen;
        popupPanel.gameObject.SetActive(true);
        popupPanel.DOScale(isOpen ? 1 : 0, 0.2f).SetEase(Ease.OutBack);

        if (!isOpen)
            DOVirtual.DelayedCall(0.2f, () => popupPanel.gameObject.SetActive(false));
    }

    public void ClosePopup()
    {
        isOpen = false;
        popupPanel.gameObject.SetActive(false);
    }
}
