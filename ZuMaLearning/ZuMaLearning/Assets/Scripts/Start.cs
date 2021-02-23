using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    public Button btn_Start;

    private void Awake()
    {
        btn_Start.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SelectLevel");
        });
    }
}
