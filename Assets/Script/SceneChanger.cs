using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    private Button gameStartButton;
    private Button ExitButton;

    [Header("--FadeInFadeOut--")]
    public Image blackPanel;
    public float fadeDuration = 1.0f;
    bool isFading = false;
    public int currentSceneindex;

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

    //void OnLevelWasLoaded()
    //{
    //    Debug.Log("SceneChanger OnLevelWasLoaded");
    //    if (!blackPanel)
    //    {
    //        blackPanel = Image.Find("BlackCanvas").transform.GetChild(0).gameObject;
    //        black.SetActive(false);
    //    }
    //}
        void Start()
    {
        if (SceneManager.GetActiveScene().name == "LogoScene")
        {
            StartCoroutine(LogoStart());
        }
    }

    IEnumerator LogoStart()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("StartScene");

        //SoundManager.instance.PlayBGM("StartBGM");
        yield return new WaitForSeconds(1.0f);
        GameObject StartButtonObj = GameObject.Find("StartBtn");
        GameObject ExitButtonObj = GameObject.Find("ExitBtn");

        if (StartButtonObj != null)
        {
            gameStartButton = StartButtonObj.GetComponent<Button>();
            gameStartButton.onClick.AddListener(OnSceneLoaded);
        }
        else
        {
            Debug.Log("StartButton is null");
        }

        if (ExitButtonObj != null)
        {
            ExitButton = ExitButtonObj.GetComponent<Button>();
            ExitButton.onClick.AddListener(GameExit);
        }
        else
        {
            Debug.Log("ExitButton is null");
        }
    }

    public void OnSceneLoaded()
    {
        Debug.Log("¾À ·Îµå");
        currentSceneindex = SceneManager.GetActiveScene().buildIndex; 
        StartCoroutine(FadeInAndLoadScene());
    }

    public void OnNowSceneReloaded()
    {
        Debug.Log("ÇöÀç ¾À Àç·Îµå");
        currentSceneindex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log(currentSceneindex);
        StartCoroutine(FadeInAndReloadScene());
    }
    public IEnumerator FadeInAndLoadScene()
    {
        isFading = true;

        yield return StartCoroutine(FadeImage(0, 1, fadeDuration)); 
        Debug.Log("Fade In");

        SceneManager.LoadScene(currentSceneindex + 1);  

        yield return StartCoroutine(FadeImage(1, 0, fadeDuration));  
        Debug.Log("Fade Out");

        // ¾À¿¡ µû¸¥ BGM Àç»ý
        PlaySceneBGM();

        isFading = false;  
    }

    public IEnumerator FadeInAndReloadScene()
    {
        isFading = true;

        yield return StartCoroutine(FadeImage(0, 1, fadeDuration));
        Debug.Log("Fade In");

        SceneManager.LoadScene(currentSceneindex);

        yield return StartCoroutine(FadeImage(1, 0, fadeDuration));
        Debug.Log("Fade Out");

        // ¾À¿¡ µû¸¥ BGM Àç»ý
        PlaySceneBGM();

        isFading = false;
    }

    public IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        Color panelColor = blackPanel.color;  

        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            

            panelColor.a = newAlpha;
            blackPanel.color = panelColor; 

            yield return null;
        }
       
        panelColor.a = endAlpha;
        blackPanel.color = panelColor;
    }

    public void GameExit()
    {
        Application.Quit();
    }

    //°¢ ¾À¿¡ ¸Â´Â ¹è°æÀ½¾Ç Àç»ý
    public void PlaySceneBGM()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "StartScene")
        {
            SoundManager.instance.PlayBGM("StartBGM");
        }
        else if (sceneName == "GameScene1")
        {
            SoundManager.instance.PlayBGM("ReadyBGM");
        }
    }
}
