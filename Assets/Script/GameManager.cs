
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
            
            Debug.Log("GameManager awake");
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
                Cursor.lockState = CursorLockMode.None;   //마우스 커서 풀린 상태
            }
            else
            {
                Continue();
                Cursor.lockState = CursorLockMode.Locked;   //마우스 커서를 잠근 상태

            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0; //게임 시간의 흐름 멈춤

        }
        else
        {
            pauseMenu = GameObject.Find("PausePanel");
            pauseMenu.SetActive(true);
            Debug.Log("PauseMenu 오브젝트가 존재하지 않습니다.");
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
        SoundManager.instance.StopBGM();
        SoundManager.instance.PlayBGM("StartBGM");
        //Application.Quit();  
    }


    public void RestartGame()
    {
        Time.timeScale = 1;
        Debug.Log("게임 재시작");
        pauseMenu.SetActive(false);

        Destroy(instance.gameObject);


        //SceneChanger.instance.OnNowSceneReloaded();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);


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

    public void GameExit()
    {
        Application.Quit();
    }
}
