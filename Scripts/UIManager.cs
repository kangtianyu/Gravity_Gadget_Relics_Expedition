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
        GUI.Label(new Rect(10, 50, 500, 60), $"W,A,S,D 移动,  1,2,3 调整魔法小工具所结附物受到的重力");
        GUI.Label(new Rect(10, 70, 500, 80), $"点按 空格 跳跃    点按 P 重置当前区域");
        GUI.Label(new Rect(10, 90, 500, 100), $"点按 G 让魔法小工具结附于眼前可结附物体");
        GUI.Label(new Rect(10, 110, 500, 120), $"长按 G 投掷魔法小工具结附于远处可结附物体");
        GUI.Label(new Rect(10, 130, 500, 140), $"点按 F 和眼前物体交互或抓取已结附的物体");
        GUI.Label(new Rect(10, 150, 500, 160), $"长按 F 和投掷已结附的物体  ");
        GUI.color = Color.yellow;
        if (DeathSentence != "")
        {
            GUI.Label(new Rect(10, 170, 500, 180), $"因为 {DeathSentence} 当前区域已重置");
        }
        GUI.color = Color.white;
    }

}
