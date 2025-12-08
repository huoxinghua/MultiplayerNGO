using _Project.Code.Gameplay.Market.Quota;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using UnityEngine;
using UnityEngine.UI;

public class QuotaCanvas : MonoBehaviour
{
    [SerializeField] private Image _confirmedQuotaBar;
    [SerializeField] private Image _daysQuotaBar;
    [SerializeField] private Image _handQuotaBar;
    private RectTransform _confirmedQuotaBarRectTransform;
    private RectTransform _daysQuotaBarRectTransform;
    private RectTransform _handQuotaBarRectTransform;
    [SerializeField] private float _fullBarWidth;
    [SerializeField] private float _updateFrequency;
    private Timer _uiUpdateTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _confirmedQuotaBarRectTransform =  _confirmedQuotaBar.GetComponent<RectTransform>();
        _daysQuotaBarRectTransform  = _daysQuotaBar.GetComponent<RectTransform>();
        
        _uiUpdateTimer = new Timer(_updateFrequency);
        _uiUpdateTimer.Start();
       // _handQuotaBarRectTransform  = _handQuotaBar.GetComponent<RectTransform>();
        
    }

    // Update is called once per frame
    void Update()
    {
        _uiUpdateTimer.TimerUpdate(Time.deltaTime);
        if (_uiUpdateTimer.IsComplete)
        {
            AdjustQuotaBar();
            _uiUpdateTimer.Reset();
        }
    }

    private void AdjustQuotaBar()
    {
        float quotaProgress = QuotaManager.Instance.QuotaProgressPercentage;
        if (float.IsNaN(quotaProgress))
            quotaProgress = 0;

        float dayProgress = QuotaManager.Instance.DayProgressPercentage;
        if (float.IsNaN(dayProgress))
            dayProgress = 0;
        
        //confirmed Size
        SetNewWidth(_confirmedQuotaBarRectTransform, Mathf.Clamp( quotaProgress * _fullBarWidth, 0, _fullBarWidth));
        
        //days size and pos
        SetNewPosition(_daysQuotaBarRectTransform, _confirmedQuotaBarRectTransform.sizeDelta.x);
        SetNewWidth(_daysQuotaBarRectTransform,  Mathf.Clamp(dayProgress * _fullBarWidth, 0, _fullBarWidth - _confirmedQuotaBarRectTransform.sizeDelta.x));
    }
    public void SetNewWidth(RectTransform barToChange, float targetWidth)
    {
        Vector2 currentSize = barToChange.sizeDelta;
        currentSize.x = targetWidth;
        barToChange.sizeDelta = currentSize;
    }
    public void SetNewPosition(RectTransform barToChange, float targetPosition)
    {
        Vector2 currentPosition = barToChange.anchoredPosition;
        currentPosition.x = targetPosition;
        barToChange.anchoredPosition = currentPosition;
    }
}
