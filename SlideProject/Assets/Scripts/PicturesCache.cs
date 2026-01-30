using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicturesCache : MonoBehaviour
{
    public static PicturesCache instance;

    public Dictionary<string, Sprite> cache { private set; get; } = new Dictionary<string, Sprite>();
    private void Awake()
    {
        instance = this;
    }

    public bool IsLoad(string key)
    {
        return cache.ContainsKey(key);
    }
}
