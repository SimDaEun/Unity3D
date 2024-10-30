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
            instance = this;
        }
        LimitTime = 50f;
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

        //Ÿ�̸Ӱ� 10�� �����̸� ���ڻ� ���������� ����
        if (LimitTime < 10)
        {
            TimerText.color = Color.red;
        }

        //Ÿ�̸Ӱ� 0�����̸� ���� ���� + Ÿ�ӿ��� �г� ���
        if (LimitTime <= 0)
        {
            Debug.Log("Time Over !");
            M_PlayerManager.instance.TimeOverPanel.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;   //���콺 Ŀ�� Ǯ�� ����
            SoundManager.instance.StopBGM();
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
