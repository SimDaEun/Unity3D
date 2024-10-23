using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class GameSettingManager : MonoBehaviour
{
    //UI Text
    public Text resolutionText;
    public Text graphicsQualityText;
    public Text fullScreenText;

    public CanvasScaler canvasScaler;

    private int resolutionIndex = 0;
    private int graphicsQualityIndex = 0;
    private bool isFullScreen = true;

    private string[] resolutions = { "800 x 600", "1280 x 720", "1920 x 1080" };
    private string[] graphicsQualityOptions = { "Low", "Normal", "High" };

    public GameObject option;
    void Start()
    {
        LoadSetting();
        UpdateResolutionText();
        UpdateGraphcsQualityText();
        UpdateFullScreenText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnResolutionLeftClick()
    {
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpdateResolutionText();
    }

    public void OnResolutionRightClick()
    {
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpdateResolutionText();

    }
    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
    }

    public void OnGraphicsLeftClick()
    {
        graphicsQualityIndex = Mathf.Max(0, graphicsQualityIndex - 1);
        UpdateGraphcsQualityText();
    }

    public void OnGraphicsRightClick()
    {
        graphicsQualityIndex = Mathf.Min(graphicsQualityOptions.Length - 1, graphicsQualityIndex + 1);
        UpdateGraphcsQualityText();
    }

    private void UpdateGraphcsQualityText()
    {
        graphicsQualityText.text = graphicsQualityOptions[graphicsQualityIndex];
    }

    public void OnFullScreenToggleClick()
    {
        isFullScreen = !isFullScreen;
        UpdateFullScreenText();
    }

    public void UpdateFullScreenText()
    {
        fullScreenText.text = "전체 화면 : " + (isFullScreen ? "켜짐" : "꺼짐");
    }

    private void ApplySettings()
    {
        string[] res = resolutions[resolutionIndex].Split('x');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height,isFullScreen);

        QualitySettings.SetQualityLevel(graphicsQualityIndex);

        //정말 변경하시겠습니까?  같은 문구가 있어야한다. 
        SaveSettings();
    }


    //세팅 저장
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQaulityIndex", graphicsQualityIndex);
        PlayerPrefs.SetInt("FullScreed",isFullScreen ? 1: 0);
        PlayerPrefs.Save(); 
    }

    private void LoadSetting()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        graphicsQualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);
        isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;
    }

    public void OptionExitButton()
    {
        option.SetActive(false);
    }
}
