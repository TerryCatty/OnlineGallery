using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

public class OptimizedGridScroll : MonoBehaviour
{
    [Header("Components")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject cellPrefab;

    [Header("Adaptive Settings")]
    public int minColumns = 1;
    public int maxColumns = 10;

    [SerializeField]
    private PicturesLoader picturesLoader;

    private List<PictureCellInfo> dataList = new List<PictureCellInfo>();
    private List<PictureCell> pool = new List<PictureCell>();
    private List<RectTransform> poolRects = new List<RectTransform>();

    private GridLayoutGroup grid;
    private int columns;
    private float rowHeight;
    private int headRowIndex = 0;
    private int lastMaxIndex = -1;

    private float cellWidth;
    private float spacingX;
    private float spacingY;
    private int paddingTop;
    private float paddingLeft;

    private Vector2 lastViewportSize;

    public event Action OnLoadMore;

    void Awake()
    {
        Application.targetFrameRate = 60;
        grid = content.GetComponent<GridLayoutGroup>();

        cellWidth = grid.cellSize.x;
        spacingX = grid.spacing.x;
        spacingY = grid.spacing.y;
        rowHeight = grid.cellSize.y + spacingY;
        paddingTop = grid.padding.top;

        SetupContentTransform();
        CalculateGrid();

        grid.enabled = false;
        PicturesLoader.loadPicture += UpdateImage;
    }

    void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScroll);
        lastViewportSize = scrollRect.viewport.rect.size;

        StartCoroutine(DelayedRefresh(0.01f));
    }
    private IEnumerator DelayedRefresh(float delay)
    {
        yield return new WaitForSeconds(delay);

        Canvas.ForceUpdateCanvases();

        if (dataList.Count > 0)
        {
            RefreshGridLayout();
        }
    }

    private void RefreshGridLayout()
    {
        CalculateGrid();
        InitializePool();
        RefreshContentHeight();
        ResetPoolToTop();
    }

    void Update()
    {
        if (scrollRect.viewport.rect.size != lastViewportSize)
        {
            lastViewportSize = scrollRect.viewport.rect.size;
            OnViewportResized();
        }
    }

    private void OnViewportResized()
    {
        int oldColumns = columns;
        CalculateGrid();

        if (oldColumns != columns && isInitialLoaded)
        {
            InitializePool();
        }

        RefreshContentHeight();
        ResetPoolToTop();
    }

    private bool isInitialLoaded = false;

    public void SetDataList(List<PictureCellInfo> newData)
    {
        dataList.Clear();
        lastMaxIndex = -1;
        content.anchoredPosition = Vector2.zero;
        headRowIndex = 0;
        UpdateDataList(newData);
        isInitialLoaded = true;
    }

    public void UpdateDataList(List<PictureCellInfo> newData)
    {
        var existingLinks = new HashSet<string>(dataList.Select(x => x.link));
        var filteredNewData = newData.Where(x => !existingLinks.Contains(x.link)).ToList();

        dataList.AddRange(filteredNewData);
        ApplyChangesData();
    }

    private void ApplyChangesData()
    {
        if (pool.Count == 0)
            InitializePool();
        else
            ResetPoolToTop();

        RefreshContentHeight();
        OnScroll(scrollRect.normalizedPosition);
    }

    private void ResetPoolToTop()
    {
        headRowIndex = Mathf.FloorToInt(content.anchoredPosition.y / rowHeight);
        headRowIndex = Mathf.Max(0, headRowIndex);

        for (int i = 0; i < pool.Count; i++)
        {
            UpdateCell(pool[i], headRowIndex * columns + i);
        }
    }

    void CalculateGrid()
    {
        float viewportWidth = scrollRect.viewport.rect.width;

        float effectiveWidth = viewportWidth - grid.padding.left - grid.padding.right;

        int calcCols = Mathf.FloorToInt((effectiveWidth + spacingX) / (cellWidth + spacingX));

        columns = Mathf.Clamp(calcCols, minColumns, maxColumns);

        float totalGridWidth = (columns * cellWidth) + ((columns - 1) * spacingX);

        paddingLeft = (viewportWidth - totalGridWidth) / 2f;
    }

    void UpdateCell(PictureCell cell, int index)
    {
        int poolIdx = pool.IndexOf(cell);
        RectTransform rt = poolRects[poolIdx];

        if (index >= 0 && index < dataList.Count)
        {
            cell.gameObject.SetActive(true);
            cell.ConfigureCell(dataList[index]);

            int row = index / columns;
            int col = index % columns;

            float x = paddingLeft + col * (cellWidth + spacingX);
            float y = -(paddingTop + row * rowHeight);

            rt.anchoredPosition = new Vector2(x, y);

            if (PicturesCache.instance != null && PicturesCache.instance.IsLoad(cell.Info.link))
                cell.SetImage(picturesLoader.GetImageFromCache(cell.Info));
            else
                picturesLoader.RequestGetImage(cell.Info);

        }
        else
        {
            cell.gameObject.SetActive(false);
        }
    }

    void InitializePool()
    {
        foreach (var c in pool) if (c != null) Destroy(c.gameObject);
        pool.Clear();
        poolRects.Clear();

        int rowsToVisible = Mathf.CeilToInt(scrollRect.viewport.rect.height / rowHeight);
        int totalToSpawn = (rowsToVisible + 2) * columns;

        for (int i = 0; i < totalToSpawn; i++)
        {
            var cell = Instantiate(cellPrefab, content).GetComponent<PictureCell>();
            var rt = cell.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            pool.Add(cell);
            poolRects.Add(rt);
        }
    }

    void OnScroll(Vector2 pos)
    {
        if (dataList.Count == 0) return;
        int currentHeadRow = Mathf.FloorToInt(content.anchoredPosition.y / rowHeight);
        int maxRow = Mathf.CeilToInt((float)dataList.Count / columns) - (pool.Count / columns);
        currentHeadRow = Mathf.Clamp(currentHeadRow, 0, Mathf.Max(0, maxRow));

        if (currentHeadRow != headRowIndex) UpdatePool(currentHeadRow);

        if (scrollRect.verticalNormalizedPosition < 0.05f && !_isUpdating)
        {
            if (pool.Count > 0 && PicturesCache.instance.IsLoad(pool[pool.Count - 1].Info.link))
            {
                _isUpdating = true;
                OnLoadMore?.Invoke();
            }
        }
        if (scrollRect.verticalNormalizedPosition > 0.1f) _isUpdating = false;
    }

    private bool _isUpdating = false;

    private void UpdatePool(int newHeadRow)
    {
        bool scrollingDown = newHeadRow > headRowIndex;
        while (newHeadRow != headRowIndex)
        {
            if (scrollingDown)
            {
                for (int i = 0; i < columns; i++)
                {
                    var cell = pool[0]; pool.RemoveAt(0); pool.Add(cell);
                    var rect = poolRects[0]; poolRects.RemoveAt(0); poolRects.Add(rect);
                    UpdateCell(cell, (headRowIndex + (pool.Count / columns)) * columns + i);
                }
                headRowIndex++;
            }
            else
            {
                headRowIndex--;
                for (int i = columns - 1; i >= 0; i--)
                {
                    var cell = pool[pool.Count - 1]; pool.RemoveAt(pool.Count - 1); pool.Insert(0, cell);
                    var rect = poolRects[poolRects.Count - 1]; poolRects.RemoveAt(poolRects.Count - 1); poolRects.Insert(0, rect);
                    UpdateCell(cell, headRowIndex * columns + i);
                }
            }
        }
    }

    public void UpdateImage(string link)
    {
        foreach (var cell in pool)
            if (cell != null && cell.gameObject.activeSelf && cell.Info.link == link)
                cell.SetImage(picturesLoader.GetImageFromCache(cell.Info));
    }

    void SetupContentTransform()
    {
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
    }

    void RefreshContentHeight()
    {
        int totalRows = Mathf.CeilToInt((float)dataList.Count / columns);
        float totalHeight = (totalRows * rowHeight) - spacingY + paddingTop + grid.padding.bottom;
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
    }
}