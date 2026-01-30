using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PicturesLoader : MonoBehaviour
{
    private Queue<PictureCellInfo> _loadCells = new Queue<PictureCellInfo>();

    [SerializeField]
    private int maxCountLoadingElements = 5;

    private bool _isLoading => maxCountLoadingElements == countLoadingElement;

    public static event Action<string> loadPicture;

    int countLoadingElement = 0;

    public void RequestGetImage(PictureCellInfo cell)
    {
        _loadCells.Enqueue(cell);

        if (_isLoading == false)
        {
            LoadQueue().Forget();
        }
    }

    public Sprite GetImageFromCache(PictureCellInfo cell)
    {
        if (PicturesCache.instance.cache.ContainsKey(cell.link))
        {
            return PicturesCache.instance.cache[cell.link];
        }

        return null;
    }

    private async UniTaskVoid LoadQueue()
    {
        PictureCellInfo picture = _loadCells.Dequeue();
        countLoadingElement++;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(picture.link))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Ошибка загрузки: " + request.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                if (PicturesCache.instance.cache.ContainsKey(picture.link) == false)
                {
                    PicturesCache.instance.cache.Add(picture.link, sprite);
                }
            }
        }

        countLoadingElement--;

        loadPicture?.Invoke(picture.link);

        if (_loadCells.Count > 0)
        {
            LoadQueue().Forget();
        }
    }
}
