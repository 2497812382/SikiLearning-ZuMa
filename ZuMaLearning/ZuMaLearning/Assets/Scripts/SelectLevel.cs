using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectLevel : MonoBehaviour
{
    public Button btn_Chapter1, btn_Chapter2, btn_Chapter3;//章节按钮
    private GameObject levelList;//关卡列表
    private Button btn_Back;//返回按钮
    public int chapterIndex, levelIndex;//章节和关卡的索引
    public int chapter_1_Count = 10;
    public int chapter_2_Count = 10;
    public int chapter_3_Count = 10;

    private void Awake()
    {
        levelList = transform.Find("LevelPanel").gameObject;
        btn_Chapter1.onClick.AddListener(() =>
        {
            chapterIndex = 0;
            levelList.SetActive(true);
        });
        btn_Chapter2.onClick.AddListener(() =>
        {
            chapterIndex = 1;
            levelList.SetActive(true);
        });
        btn_Chapter3.onClick.AddListener(() =>
        {
            chapterIndex = 2;
            levelList.SetActive(true);
        });
        //别忘了返回按钮是关卡列表的子物体
        btn_Back = levelList.transform.Find("Btn_Back").gameObject.GetComponent<Button>();
        btn_Back.onClick.AddListener(() =>
        {
            levelList.SetActive(false);
        });

        Transform list = levelList.transform.Find("LevelList");//获取关卡列表
        //遍历关卡列表中的关卡按钮，注册点击事件
        for(int i = 0; i<list.childCount; i++)
        {
            int index = i;
            list.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
            {
                levelIndex = index;
                if(chapterIndex == 0)
                {
                    GameData.levelIndex = levelIndex;
                }
                if (chapterIndex == 1)
                {
                    GameData.levelIndex = 10 * chapterIndex + levelIndex;
                }
                if (chapterIndex == 2)
                {
                    GameData.levelIndex = 10 * chapterIndex + levelIndex;
                }
                SceneManager.LoadScene("Game");
            });
        }
    }
}
