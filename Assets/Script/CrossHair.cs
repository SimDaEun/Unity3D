using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    private RectTransform crossHair;

    private float crossHairDefaultSize = 50;  //�⺻ ������
    private float crossHairSize = 50;  //������ CrossHairSize 
    private float crossHairSpeed = 50;  //CrossHair Ȯ�� �ӵ�
    public float crossHairMaxSize = 500;  //crossHair �ִ������

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

        crossHair.sizeDelta = new Vector2(crossHairSize, crossHairSize);  //ũ�� ����
    }
}
