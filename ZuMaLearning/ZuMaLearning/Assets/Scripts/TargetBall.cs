using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBall : MonoBehaviour
{
    public float position = 0f;//目标球的显示进程，可以理解为我要显示第几个球了。
    private GameManager gameManager;
    public BallType ballType;//球的类型
    private SpriteRenderer spriteRenderer;
    public bool isMatch = false;//是否消除的标志
    public TargetBall connectTarget;//上一段的连接球

    //获取SpriteRenderer组件
    public void InitSpriteRenderer()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    //初始化目标球，把需要的变量赋值上
    public TargetBall Init(GameManager gm, BallType type)
    {
        ballType = type;
        gameManager = gm;
        Sprite sp = gameManager.GetSpriteByBallType(ballType);
        if (sp != null)
            spriteRenderer.sprite = sp;
        transform.position = new Vector3(10, 10, 10);
        gameObject.SetActive(true);
        position = 0f;
        NextBall = null;
        PreBall = null;
        isMatch = false;
        return this;
    }
    private void Update()
    {
        //mapConfig是赋值在GameManager上的，所以要通过GM来获取mapConfig。
        //通过调用mapConfig上的获取具体位置的方法来得到当前目标球的位置。
        if(position >= 0)
        transform.localPosition = gameManager.mapConfig.GetPosition(position);
    }
    //是否已经不是起始点的球
    public bool IsNotStartBall()
    {
        return position >= 1f;
    }
    public TargetBall PreBall { get; set; }//前驱点的球
    public TargetBall NextBall { get; set; }//后继点的球
    //读一个队列段最后一个球（最靠近终点的球）
    public TargetBall LastBall
    {
        get
        {
            TargetBall ball = this;
            do
            {
                if (ball.NextBall == null)
                    return ball;
                ball = ball.NextBall;
            } while (true);
        }
    }
    //读一个队列段第一个球
    public TargetBall HeadBall
    {
        get
        {
            TargetBall ball = this;
            do
            {
                if (ball.PreBall == null)
                    return ball;
                ball = ball.PreBall;
            } while (true);
        }
    }
    //判断显示进度是否移动到等于终点球的下标
    public bool isToLastPoint()
    {
        return position >= gameManager.mapConfig.LastPoint;
    }
    //判定连续相邻球同色的数量
    //传递一个放置同色球的列表出去
    public int SameColorAmount(out List<TargetBall> matchList)
    {
        matchList = new List<TargetBall>();
        matchList.Add(this);
        int counter = 1;//同色球数计数器
        TargetBall curretBall = this;
        //向前判断
        while(curretBall != null)
        {   //当前球的前驱不为空且前驱的颜色和当前球一样时，说明两个相邻球同色，则继续遍历
            if (curretBall.PreBall != null && curretBall.PreBall.ballType == ballType)
            {
                matchList.Add(curretBall.PreBall);
                curretBall = curretBall.PreBall;
                counter++;
            }
            else
                break;
        }
        //向后判断
        curretBall = this;
        while (curretBall != null)
        {   //当前球的后继不为空且后继的颜色和当前球一样时，说明两个相邻球同色，则继续遍历
            if (curretBall.NextBall != null && curretBall.NextBall.ballType == ballType)
            {
                matchList.Add(curretBall.NextBall);
                curretBall = curretBall.NextBall;
                counter++;
            }
            else
                break;
        }
        return counter;
    }
    //目标球回收，用于消除操作
    public void Recall()
    {
        gameManager.targetBallPool.AddObject(this);
        gameObject.SetActive(false);
    }
}
