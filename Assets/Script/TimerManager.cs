using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public static TimerManager instance;

    public float LimitTime;
    public Text TimerText;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    private void Update()
    {
        if (!M_PlayerManager.instance.isFloating)
        {
            ChangeTime();
        }
        else
        {
            StopTime();
        }

        //타이머가 10초 이하이면 글자색 빨간색으로 변경
        if (LimitTime < 10)
        {
            TimerText.color = Color.red;
        }

        //타이머가 0이하이면 게임 멈춤 + 타임오버 패널 띄움
        if (TimerManager.instance.LimitTime <= 0)
        {
            M_PlayerManager.instance.TimeOverPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    private void ChangeTime()
    {
        LimitTime -= Time.deltaTime;
        TimerText.text = $"{Mathf.Round(LimitTime)}";
    }

    private void StopTime()
    {
        TimerText.text = $"{Mathf.Round(LimitTime)}";
    }

}
