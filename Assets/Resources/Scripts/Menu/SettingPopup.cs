using DG.Tweening;
using UnityEngine;

public class SettingPopup : MonoBehaviour
{
    [SerializeField] private RectTransform popupPanel;
    [SerializeField] private GameObject settingButton;

    private bool isOpen = false;

    public void TogglePopup()
    {
        isOpen = !isOpen;
        AudioManager.Ins.PlaySfx(isOpen ? SfxCue.UiPopupOpen : SfxCue.UiPopupClose);

        if (settingButton != null)
            settingButton.SetActive(!isOpen);

        popupPanel.gameObject.SetActive(true);
        popupPanel.DOScale(isOpen ? 1 : 0, 0.2f).SetEase(Ease.OutBack);

        if (!isOpen)
            DOVirtual.DelayedCall(0.2f, () => popupPanel.gameObject.SetActive(false));
    }

    public void ClosePopup()
    {
        isOpen = false;
        AudioManager.Ins.PlaySfx(SfxCue.UiPopupClose);
        popupPanel.gameObject.SetActive(false);
        if (settingButton != null)
            settingButton.SetActive(true);
    }
}
