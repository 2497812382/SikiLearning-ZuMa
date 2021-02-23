using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager _instance;
    public GameObject gameOverPanel;
    public GameObject levelClearPanel;

    private void Awake()
    {
        _instance = this;
        //注册点击事件
        gameOverPanel.transform.Find("Btn_Restart").GetComponent<Button>().onClick.AddListener(()=>
        {
            gameOverPanel.SetActive(false);
            GameManager._instance.BackState();
        });
        gameOverPanel.transform.Find("Btn_Replay").GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        gameOverPanel.transform.Find("Btn_Home").GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Start");
        });
        levelClearPanel.transform.Find("Btn_Home").GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Start");
        });
        levelClearPanel.transform.Find("Btn_Next").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameData.levelIndex += 5;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }
    public void ShowFailed()
    {
        gameOverPanel.SetActive(true);
    }
    public void ShowPass()
    {
        levelClearPanel.SetActive(true);
    }
}
