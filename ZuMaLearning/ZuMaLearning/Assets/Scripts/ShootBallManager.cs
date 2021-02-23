using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBallManager : MonoBehaviour
{
    public static ShootBallManager _instance;
    private ObjectPool<ShootBall> shootPool;
    public GameObject ballPrefab;
    public List<ShootBall> shootedBallList = new List<ShootBall>();//已经发射的球的集合

    private void Awake()
    {
        _instance = this;
        shootPool = new ObjectPool<ShootBall>(InstanceObject, 4);
    }
    private void Update()
    {
        //遍历已发射球列表，超出边界的就调用回收方法
        for (int i = shootedBallList.Count - 1; i >= 0; i--)
        {
            shootedBallList[i].Move();
            if (shootedBallList[i].IsOutOfBoundary())
            {
                Recall(shootedBallList[i]);
                shootedBallList.RemoveAt(i);
            }
        }
    }
    //实例化球的预制体，并添加脚本
    private ShootBall InstanceObject()
    {
        GameObject ball = Instantiate(ballPrefab, transform);
        ball.SetActive(false);
        ShootBall shootBall = ball.AddComponent<ShootBall>();
        return shootBall;
    }
    //发射球
    public void Shoot(BallType type, Sprite sp, Transform tr)
    {
        ShootBall shootBall = shootPool.GetObject();
        shootBall.Init(type,sp,tr);
        shootedBallList.Add(shootBall);
    }
    //对象池回收发射球
    public void Recall(ShootBall ball)
    {
        ball.gameObject.SetActive(false);
        shootPool.AddObject(ball);
    }
}
