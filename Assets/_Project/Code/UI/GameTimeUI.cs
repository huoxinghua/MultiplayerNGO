using System;
using _Project.Code.Gameplay;
using _Project.Code.Utilities.EventBus;
using _Project.ScriptableObjects.ScriptObjects.GameTime;
using TMPro;
using UnityEngine;

public class GameTimeUI : MonoBehaviour
{
    //1080 "minutes" will take the time from 9am - 3am
    //900 seconds will take 15 real minutes
    //1.2 seconds should equal one game minute than
    [SerializeField] private TMP_Text _gameTimeText;
    [SerializeField] private GameTimeVisualSO  _gameTimeVisualSO;
    private int _totalHours;
    private int _totalMinutes;
        //minutes an hour, or seconds a minute
    const int minPH = 60;
    //for standard 12 hour time
    const int _tPmAm = 12;
    //for 24 hour time
    const int _tMil = 24;
    private void Awake()
    {
        EventBus.Instance.Subscribe<GameTimeTickedEvent>(this, SetGameTimeText);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameTimeTickedEvent>(this);
    }

    public void SetGameTimeText(GameTimeTickedEvent gameTimeTickedEvent)
    {
        string amPm = "";
      
        _totalMinutes = Mathf.FloorToInt(gameTimeTickedEvent.GameTime * _gameTimeVisualSO.SecondsToGameMinutes);
        _totalHours = Mathf.FloorToInt(_totalMinutes / 60);
        amPm = GetAmPm();
        int displayHour =(_gameTimeVisualSO.StartTimeMilitaryTime + _totalHours -1 )% 12 +1;
        int displayMinute = _totalMinutes % 60;
            _gameTimeText.SetText($"{displayHour}:{displayMinute:00} {amPm}");
        
    }

    public string GetAmPm()
    {
        float startPlusElapsed = _gameTimeVisualSO.StartTimeMilitaryTime + _totalHours;
        if ((startPlusElapsed) % 24 >= 12)
        {
            return "PM";
        }
        else
        {
            return "AM";
        }
    }
}
