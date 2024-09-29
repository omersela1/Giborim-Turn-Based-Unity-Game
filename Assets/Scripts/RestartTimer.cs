using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RestartTimer : MonoBehaviour
{
    #region Variables

    private static RestartTimer instance;

    public static RestartTimer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.Find("RestartTimer").GetComponent<RestartTimer>();
            }

            return instance;
        }
    }

    private float _time;
    public TextMeshProUGUI textObject;
    public static event Action RestartTimerZero;
    
    #endregion
    
    #region Initialization
    void OnEnable()
    {
        _time = 6f;
    }

    void OnDisable()
    {
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
                RestartTimerZero?.Invoke();
            }
        }
    }

    public void Restart()
    {
        _time = 6f;
    }

    void StopTimer(string winner)
    {
        gameObject.SetActive(false);
    }
    
    #endregion
}

