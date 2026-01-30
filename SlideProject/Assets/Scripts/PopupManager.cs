using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [SerializeField]
    private Transform popupParent;

    public PicturePopup pictureShowPrefab;

    public PremiumPopup premiumShowPrefab;

    public static PopupManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    public TWindow OpenPopup<TWindow, TData>(TWindow prefab, TData data)
        where TWindow : BasePopup<TData>
        where TData : class
    {
        TWindow instance = Instantiate(prefab, popupParent);
        instance.Initialize(data);

        return instance;
    }
   
    public void ClosePopup<T>(BasePopup<T> window) where T : class
    {
        Destroy(window.gameObject);
    }
}
