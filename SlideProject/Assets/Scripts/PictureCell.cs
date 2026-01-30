using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PictureCell : MonoBehaviour
{
    public Image photoDisplay;

    [SerializeField]
    private Image premiumBadge;

    [SerializeField]
    private Image loadingImage;
    public PictureCellInfo Info { get; private set; }

    public void ConfigureCell(PictureCellInfo info)
    {
        Info = info;

        CheckIsPremium();
        photoDisplay.gameObject.SetActive(false);
        loadingImage.gameObject.SetActive(true);
    }


    public void SetImage(Sprite sprite)
    {
        photoDisplay.sprite = sprite;

        loadingImage.gameObject.SetActive(false);
        photoDisplay.gameObject.SetActive(true);
    }

    private void CheckIsPremium()
    {
        premiumBadge.gameObject.SetActive(Info.isPremium);
    }

    public void Click()
    {
        if (PicturesCache.instance.IsLoad(Info.link) == false) return;

        PopupManager.Instance.OpenPopup(PopupManager.Instance.pictureShowPrefab, this);
    }

}
