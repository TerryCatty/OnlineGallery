using UnityEngine;
using UnityEngine.UI;

public class PicturePopup : BasePopup<PictureCell>
{
    [SerializeField]
    private Image image;

    public override void Initialize(PictureCell data)
    {
        if (data.Info.isPremium == false)
        {
            image.sprite = data.photoDisplay.sprite;
        }
        else
        {
            PopupManager.Instance.OpenPopup(PopupManager.Instance.premiumShowPrefab, new PremiumInfo()).onClose += CloseWindow;
        }
    }
}
