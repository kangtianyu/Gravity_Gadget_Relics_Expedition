using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;  // Static reference to access UIManager globally
    [HideInInspector]
    public Canvas mainCanvas;
    [HideInInspector]
    public HashSet<GameObject> selectedObjectList = new HashSet<GameObject>();
    public string DeathSentence = "";


    // Start is called before the first frame update
    void Start()
    {
        // Ensure only one instance of UIManager exists (Singleton pattern)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes it persistent across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one UIManager exists
        }

        mainCanvas = this.GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        // reset selected objects
        selectedObjectList = new HashSet<GameObject>();
    }


    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(10, 50, 500, 60), $"W,A,S,D �ƶ�,  1,2,3 ����ħ��С�������ḽ���ܵ�������");
        GUI.Label(new Rect(10, 70, 500, 80), $"�㰴 �ո� ��Ծ    �㰴 P ���õ�ǰ����");
        GUI.Label(new Rect(10, 90, 500, 100), $"�㰴 G ��ħ��С���߽ḽ����ǰ�ɽḽ����");
        GUI.Label(new Rect(10, 110, 500, 120), $"���� G Ͷ��ħ��С���߽ḽ��Զ���ɽḽ����");
        GUI.Label(new Rect(10, 130, 500, 140), $"�㰴 F ����ǰ���彻����ץȡ�ѽḽ������");
        GUI.Label(new Rect(10, 150, 500, 160), $"���� F ��Ͷ���ѽḽ������  ");
        GUI.color = Color.yellow;
        if (DeathSentence != "")
        {
            GUI.Label(new Rect(10, 170, 500, 180), $"��Ϊ {DeathSentence} ��ǰ����������");
        }
        GUI.color = Color.white;
    }

}
