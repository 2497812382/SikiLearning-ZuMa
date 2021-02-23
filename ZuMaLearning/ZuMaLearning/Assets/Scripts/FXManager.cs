using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager _instance;
    public GameObject disappearFXPrefab;
    public ObjectPool<GameObject> disappearFXPool;
    private void Awake()
    {
        _instance = this;
        disappearFXPool = new ObjectPool<GameObject>(InstanceFxObject, 8);
    }
    private GameObject InstanceFxObject()
    {
        GameObject fx = Instantiate(disappearFXPrefab, transform);
        fx.SetActive(false);
        return fx;
    }
    //根据消除目标球的位置来显示特效
    public void ShowDisappearFX(Vector3 ballPos)
    {
        GameObject fx = disappearFXPool.GetObject();
        fx.SetActive(true);
        fx.transform.localPosition = ballPos;

        //延时执行预制体隐藏和对象池回收
        //Lambda表达式是把函数当成一个可执行的对象。
        ScheduleOnce.Start(this, () =>
        {
            fx.SetActive(false);
            disappearFXPool.AddObject(fx);
        }, 0.5f);
    }
}
