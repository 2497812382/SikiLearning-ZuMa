using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallStrategy : MonoBehaviour
{
    static int constantTime = 0;//连续生成同一种球的次数
    static int ballType = 0;//球的类型
    public static int BombDestroyAmount = 5;//炸弹球销毁数量

    //目标球生成方法
    public static BallType GenerateBallType()
    {
        if(constantTime <= 0)
        {
            constantTime = Random.Range(1, 4);//给连续生成球数一个随机数量
            ballType = Random.Range(0, 4);//一共4种类型，随机去一种
        }
        constantTime--;//每生成一个球就把连续次数减一
        return (BallType)ballType;
    }
    //发射球生成方法
    public static BallType GenerateShootBallType()
    {
        return (BallType)Random.Range(0, 5);
    }
    //根据关卡号来生成球数
    public static int BallAmount(int levelIndex)
    {
        return GameData.levelIndex * 5 + 50;
    }
}
