using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    #region Variables

    private static Timer instance;

    public static Timer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.Find("Timer").GetComponent<Timer>();
            }

            return instance;
        }
    }
    private float _time, _startTime;
    public TextMeshProUGUI textObject;
    public static event Action TimerZero;
    
    #endregion
    
    #region Initialization
    void OnEnable()
    {
        GameManager.RestartTimer += Restart;
        Battleground.CallWinner += StopTimer;
        _time = 11f;
    }

    void OnDisable()
    {
        GameManager.RestartTimer -= Restart;
        Battleground.CallWinner -= StopTimer;
    }
    
    #endregion
    
    #region Logic
    void Update()
    {
        if (gameObject.activeSelf)
        {
            _time -= Time.deltaTime;
            textObject.text = ((int)_time).ToString();
            if ((int)_time == 0)
            {
                TimerZero?.Invoke();
            }
        }
    }

    public void Restart()
    {
        _time = 11f;
    }

    void StopTimer(string winner)
    {
        gameObject.SetActive(false);
    }

    public void SetStartTime(float time)
    {
        _startTime = time;
    }
    
    #endregion
}
