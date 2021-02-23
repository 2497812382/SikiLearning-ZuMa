using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public Sprite[] mapBg;//关卡地图图片
    public MapConfig[] mapData;//关卡配置文件
    public Vector3[] shooters;//发射器
    public Transform transform;//发射器的transform
    public SpriteRenderer spriteRenderer;//关卡图片渲染器

    private void Awake()
    {
        Sprite sp = mapBg[GameData.levelIndex];
        spriteRenderer.sprite = sp;
        MapConfig mapConfig = mapData[GameData.levelIndex];
        GetComponent<GameManager>().mapConfig = mapConfig;
        transform.position = shooters[GameData.levelIndex];
    }
}
