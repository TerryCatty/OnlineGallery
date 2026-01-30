using System.Collections.Generic;
using System.Linq; // Нужно для фильтрации
using UnityEngine;

public enum FilterType { All, Even, Odd }

public class PicturesManager : MonoBehaviour
{
    [SerializeField] private string link;
    [SerializeField] private string fileExtension;
    [SerializeField] private int countStartLoad = 9;
    [SerializeField] private int countLoad = 3;
    [SerializeField] private int maxCountLoaded = 66;
    [SerializeField] private OptimizedGridScroll scroll;

    private List<PictureCellInfo> _allPictures = new List<PictureCellInfo>();
    private FilterType _currentFilter = FilterType.All;
    private FilterType _lastFilter = FilterType.All;

    private void Start() 
    {
        scroll.OnLoadMore += HandleLoadMore;
        GenerateMoreData(countStartLoad);
    } 

    public void SetFilter(int filterIndex)
    {
        if(_currentFilter == (FilterType)filterIndex) return;

        _currentFilter = (FilterType)filterIndex;
        ApplyFilterAndRefresh();
    }
    private void HandleLoadMore()
    {
        GenerateMoreData(countLoad);
    }

    public void GenerateMoreData(int amount)
    {
        if (_allPictures.Count >= maxCountLoaded) return;

        int startIndex = _allPictures.Count;


        for (int i = startIndex; i < startIndex + amount; i++)
        {
            int displayId = i + 1;
            string fullLink = $"{link}/{displayId}.{fileExtension}";

            _allPictures.Add(new PictureCellInfo
            {
                link = fullLink,
                isPremium = displayId % 4 == 0,
                id = displayId
            });

            if (_allPictures.Count >= maxCountLoaded) break;
        }
        ApplyFilterAndRefresh();
    }

    private void ApplyFilterAndRefresh()
    {
        List<PictureCellInfo> filtered;

        switch (_currentFilter)
        {
            case FilterType.Even:
                filtered = _allPictures.Where((x, index) => (index + 1) % 2 == 0).ToList();
                break;
            case FilterType.Odd:
                filtered = _allPictures.Where((x, index) => (index + 1) % 2 != 0).ToList();
                break;
            default:
                filtered = new List<PictureCellInfo>(_allPictures);
                break;
        }


        if(_lastFilter == _currentFilter)
        {
            scroll.UpdateDataList(filtered);
        }
        else
        {
            scroll.SetDataList(filtered);
            _lastFilter = _currentFilter;
        }
    }
}