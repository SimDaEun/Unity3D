using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    private RectTransform crossHair;

    private float crossHairDefaultSize = 50;  //기본 사이즈
    private float crossHairSize = 50;  //변경할 CrossHairSize 
    private float crossHairSpeed = 50;  //CrossHair 확대 속도
    public float crossHairMaxSize = 500;  //crossHair 최대사이즈

    void Start()
    {
        crossHair = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossHairMaxSize, Time.deltaTime * crossHairSpeed);
        }
        else
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossHairDefaultSize, Time.deltaTime * 2);
        }

        crossHair.sizeDelta = new Vector2(crossHairSize, crossHairSize);  //크기 적용
    }
}
