using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject pauseMenu;
    public GameObject HelpImage;
    private bool isPaues = false;

    public GameObject DefaultPos;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("awake");
        }
        else
        {
            Debug.Log("instance is not null");
            Destroy(gameObject);
        }
        
    }

    void Start()
    {
        M_PlayerManager.instance.gameObject.transform.position = DefaultPos.transform.position;
        pauseMenu.SetActive(false);
        HelpImage.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaues = !isPaues;
            if (isPaues)
            {
                Debug.Log("Pause !");
                PauseGame();
                Cursor.lockState = CursorLockMode.None;   //���콺 Ŀ�� Ǯ�� ����
            }
            else
            {
                Continue();
                Cursor.lockState = CursorLockMode.Locked;   //���콺 Ŀ���� ��� ����

            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0; //���� �ð��� �帧 ����

        }
        else
        {
            pauseMenu = GameObject.Find("PausePanel");
            pauseMenu.SetActive(true);
            Debug.Log("PauseMenu ������Ʈ�� �������� �ʽ��ϴ�.");
        }
    }
    public void Continue()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            Debug.Log("Continue");
        }
    }

    public void Exit()
    {
        Debug.Log("Exit");
        SceneManager.LoadScene("StartScene");
        //Application.Quit();  
    }


    public void RestartGame()
    {
        Time.timeScale = 1;
        Debug.Log("���� �����");
        pauseMenu.SetActive(false);

        Destroy(instance.gameObject);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        M_PlayerManager.instance.gameObject.GetComponent<CharacterController>().enabled = false;
        M_PlayerManager.instance.gameObject.transform.position = DefaultPos.transform.position;
        M_PlayerManager.instance.gameObject.GetComponent<CharacterController>().enabled = true;
        //SoundManager.instance.PlayBGM(currentScene.name+"BGM");
    }
    public void Help()
    {
        HelpImage.SetActive(true);
    }
    public void CloseHelp()
    {
        HelpImage.SetActive(false);
    }
}