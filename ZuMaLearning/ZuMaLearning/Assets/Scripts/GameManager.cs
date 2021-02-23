using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

//球的类型
public enum BallType
{
    Red,
    Blue,
    Green,
    Yellow,
    Bomb
}
//游戏状态
public enum GameState
{
    Game,
    Success,
    Fail
}
//球的移动状态
public enum MoveState
{
    Forward,
    Back
}

//创建一个关联类型和球图片的关系类
//记得复习下之前消消乐的做法
//序列化以方便外部赋值
[System.Serializable]
public class TypeAndSprite
{
    public BallType ballType;
    public Sprite ballSprite;
}
public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    public MapConfig mapConfig;//关卡文件
    public GameObject targetBallPrefab;//目标球预制体
    public List<TargetBall> targetBallSegementList = new List<TargetBall>();//目标球队列，这里是用队列的第一个球来表示本段
    public float moveSpeed = 2f;//目标球移动速度
    public List<TypeAndSprite> typeAndSpritesList = new List<TypeAndSprite>();//类型和图片的关系列表
    private Dictionary<BallType, Sprite> typeAndSpritesDic = new Dictionary<BallType, Sprite>();//类型和图片的关系字典

    public ObjectPool<TargetBall> targetBallPool;//目标球的对象池
    public  List<TargetBall> insertList = new List<TargetBall>();//插入球的标记列表
    private List<TargetBall> connectBallList = new List<TargetBall>();//下一段的连接球列表

    private bool isBomb = false;//是否为炸弹球
    private GameState gameState = GameState.Game;//默认为游戏状态
    private MoveState moveState = MoveState.Forward;//默认为向前
    private void Awake()
    {
        _instance = this;
        //向字典里添加关系
        typeAndSpritesDic = new Dictionary<BallType, Sprite>();
        foreach (var item in typeAndSpritesList)
        {
            typeAndSpritesDic.Add(item.ballType, item.ballSprite);
        }
    }
    private IEnumerator Start()
    {
        targetBallPool = new ObjectPool<TargetBall>(InstanceTargetBallFunc, 100);//赋值对象池
        mapConfig.InitLastPoint();//初始化终点球下标

        //播放BGM，先获取播放所需的AudioSource组件，然后获取clip，进行播放
        AudioSource bgAudio = gameObject.GetComponent<AudioSource>();
        bgAudio.clip = SoundManager.GetSoundClip("Bg");
        bgAudio.loop = true;
        bgAudio.volume = 0.5f;
        bgAudio.Play();

        //开局快速移动，持续1s后恢复正常速度
        moveSpeed = 10f;
        SoundManager.PlayFastMove();
        yield return new WaitForSeconds(1f);
        moveSpeed = 2f;
    }
    private void Update()
    {
        if(gameState == GameState.Game)
        {
            if(moveState == MoveState.Forward)
            {
                FirstBallMove();
                CheckIsOver();
            }
            else if(moveState == MoveState.Back)
            {
                BallBack();
            }
            InsertShootBall();
            MatchBall();
            ExecuteBack();
            ExecuteConnection();
        }
    }
    //实例化目标球，给它挂载脚本的委托
    private TargetBall InstanceTargetBallFunc()
    {
        GameObject targetBall = Instantiate(targetBallPrefab, transform.Find("TargetBalls"));//实例化预制体
        targetBall.SetActive(false);
        TargetBall targetBallClass = targetBall.AddComponent<TargetBall>();//给预制体挂载目标球脚本
        targetBallClass.InitSpriteRenderer();//获取SpriteRenderer组件
        return targetBallClass;
    }
    //第一段队列球的移动
    private void FirstBallMove()
    {
        int ballAmount = BallStrategy.BallAmount(SceneManager.GetActiveScene().buildIndex);//根据场景索引获取本关球数
        //如果队列为空，那么说明还没开始出现球，就生成一段
        if (targetBallPool.ballAmount < ballAmount && targetBallSegementList.Count == 0)
        {
            targetBallPool.ballAmount++;
            //调用对象池里的获取物体方法获取到目标球脚本，然后调用目标球脚本中的初始化方法
            TargetBall targetBall = targetBallPool.GetObject().Init(this, BallStrategy.GenerateBallType());
            targetBallSegementList.Add(targetBall);
            return;
        }
        //判断游戏结束
        if (targetBallSegementList.Count <= 0)
        {
            gameState = GameState.Success;
            UIManager._instance.ShowPass();
            Debug.Log("success");
            return;
        }
        //否则就将列表的第一段赋值给它。
        TargetBall firstBall = targetBallSegementList[0];
        //第一段队列的第一个球如果已经移动了，那么就要继续生成后续的球
        if(targetBallPool.ballAmount < ballAmount && firstBall.IsNotStartBall())
        {
            targetBallPool.ballAmount++;
            TargetBall ball = targetBallPool.GetObject().Init(this, BallStrategy.GenerateBallType());//取出一个球
            //建立和后一个球的关系，类似于链表建立节点关系
            ball.NextBall = firstBall;
            firstBall.PreBall = ball;
            targetBallSegementList[0] = ball;//添加到目标球队列中
            firstBall = ball;//替换新的起始点球变量
        }
        firstBall.position += Time.deltaTime * moveSpeed;
        //只要一个球后面还有球，说明它还没到终点，可以移动。
        //循环移动后面的球
        while(firstBall.NextBall != null)
        {
            if(firstBall.NextBall.position < firstBall.position + 1)
            {
                //通过替换显示进度来实现向前移动的效果。
                firstBall.NextBall.position = firstBall.position + 1;
            }
            //每移动一次都将自己设置为自己的后面一个球，保证循环向前移动。
            firstBall = firstBall.NextBall;
        }
    }
    //检测游戏是否结束
    private void CheckIsOver()
    {
        int count = targetBallSegementList.Count;//看看当前一共有几段
        if (count == 0)
            return;
        TargetBall lastSegement = targetBallSegementList[count - 1];//找到最后一段
        //调用最后一段最后一个球的终点判断方法
        if(lastSegement.LastBall.isToLastPoint())
        {
            SoundManager.PlayFail();
            gameState = GameState.Fail;
            UIManager._instance.ShowFailed();
            Debug.Log("fail");
        }
    }
    //通过类型值来获取sprite
    public Sprite GetSpriteByBallType(BallType type)
    {
        if (typeAndSpritesDic.ContainsKey(type) == false)
            return null;
        return typeAndSpritesDic[type];
    }
    //发射球插入目标球队列
    //在这里写是因为涉及到发射InsertShootBall()球和目标球的交互，而不是某个球的单独处理
    private void InsertShootBall()
    {
        bool isInsert = false;//是否已插入
        float distance = 0.3f;//可插入距离
        List<ShootBall> shootBallList = ShootBallManager._instance.shootedBallList;//用单例获取发射球列表
        //让每个发射球都和每个目标球进行距离检测
        int i = shootBallList.Count - 1;
        //第一层：遍历发射球
        while(i >= 0)
        {
            ShootBall shootBall = shootBallList[i];
            int j = targetBallSegementList.Count - 1;
            //第二层：遍历目标球队列
            while(j >= 0)
            {
                TargetBall firstBall = targetBallSegementList[j];
                //第三层：遍历目标球队列中的每个目标球
                do
                {
                    if (shootBall.IsInsert(firstBall.transform.position, distance))
                    {
                        //如果是不是炸弹球执行正常的插入
                        if(shootBall.ballType != BallType.Bomb)
                        {
                            //所谓插入即是隐藏发射球，同时生成一个新的目标球
                            //生成一个目标球，类型和发射球的一样
                            TargetBall insertBall = targetBallPool.GetObject().Init(this, shootBall.ballType);
                            //接下来处理连接关系，和链表插入一样，对前驱和后继发生改变的地方进行重新赋值
                            //令插入球的前驱为1，自己为2，后继为3
                            TargetBall insertNext = firstBall.NextBall;//插入前1的后继为3
                            firstBall.NextBall = insertBall;//1的后继为2
                            insertBall.PreBall = firstBall;//相互的，2的前驱为1
                            insertBall.NextBall = insertNext;//2的后继为3
                                                             //处理插入球是插在了队列最后一个球的后面
                            if (insertNext != null)
                            {
                                insertNext.PreBall = insertBall;//3的前驱为1
                            }
                            insertBall.position = firstBall.position + 1;//插入后相当于移动了一步
                            insertList.Add(insertBall);//为插入球列表添加当前的插入球。
                            isInsert = true;
                        }
                        else//是炸弹球的话就把附近的球都标记为消除，前后各删除2个
                        {
                            firstBall.isMatch = true;//把自身标记为消除
                            int matchAmount = BallStrategy.BombDestroyAmount / 2;//消除总数量的一半
                            //接下来先删除前面两个
                            TargetBall ball = firstBall.PreBall;
                            while(ball != null && matchAmount > 0)
                            {
                                ball.isMatch = true;
                                ball = ball.PreBall;
                                matchAmount--;
                            }
                            //然后再删除后面两个
                            matchAmount = BallStrategy.BombDestroyAmount / 2;
                            ball = firstBall.NextBall;
                            while (ball != null && matchAmount > 0)
                            {
                                ball.isMatch = true;
                                ball = ball.NextBall;
                                matchAmount--;
                            }
                            SoundManager.PlayBomb();
                            isBomb = true;
                        }
                        shootBallList.RemoveAt(i);//将该发射球移出列表。
                        ShootBallManager._instance.Recall(shootBall);//回收发射球
                        break;
                    }
                    firstBall = firstBall.NextBall;
                } while (firstBall != null);
                //跳出外层循环
                if (isInsert)
                {
                    UPdateConnectBallPosition(targetBallSegementList[j]);
                    break;
                }
                if (isBomb)
                    break;
                j--;
            }
            i--;
        }
    }
    //消除
    private void MatchBall()
    {
        bool hasMatched = false;//是否已经被标记为消除
        int i = insertList.Count - 1;
        while(i >= 0)
        {
            List<TargetBall> matchList;//用来接收同色球的列表
            TargetBall targetBall = insertList[i];//将插入球逐一取出来进行判断
            //同色球的数量大于等于3时即满足消除条件，将它们打上标记
            if(targetBall.SameColorAmount(out matchList) >= 3)
            {
                hasMatched = true;
                foreach (var item in matchList)
                {
                    item.isMatch = true;
                }
                SoundManager.PlayEliminate();
            }
            else
            {
                SoundManager.PlayBallEnter();
            }
            i--;
            //Debug.Log(hasMatched);
        }
        insertList.Clear();
        if (hasMatched == false && isBomb == false)
            return;
        isBomb = false;
        //接下来处理消除，思路和判断同色球差不多
        //先遍历队列段
        //int j = targetBallSegementList.Count - 1;
        int j = targetBallSegementList.Count;
        while (j > 0)
        {
            j--;//放到前面来，放到continue后面会死循环
            TargetBall firstBall = targetBallSegementList[j];//通过每段的第一个球开始遍历
            TargetBall headBall = firstBall.HeadBall;//这一段第一个球
            TargetBall lastBall = firstBall.LastBall;//这一段最后一个球
            bool isDivide = false;//是否发生消除分裂
            //Debug.Log(firstBall);
            while (firstBall != null)
            {
                //如果是消除球，就断链，然后回收
                if(firstBall.isMatch)
                {
                    isDivide = true;
                    //如果这个球的前驱有球，那么就把它前驱的后继（也就是这个球）置空
                    if (firstBall.PreBall != null)
                        firstBall.PreBall.NextBall = null;
                    //如果这个球的后继有球，那么就把它后继的前驱（也就是这个球）置空
                    if (firstBall.NextBall != null)
                        firstBall.NextBall.PreBall = null;

                    //接下来处理消除完球队列分裂的情况，首先给出头尾为空的判断条件
                    //如果消除球是第一个球，那么就把它的第一个球置空
                    if (firstBall == headBall)
                        headBall = null;
                    //如果消除球是最后一个球，那么就把它的最后一个球置空
                    if (firstBall == lastBall)
                        lastBall = null;
                    //播放消除特效
                    FXManager._instance.ShowDisappearFX(firstBall.transform.position);
                    //回收球
                    firstBall.Recall();
                }
                firstBall = firstBall.NextBall;
            }

            //注意这里不能return，这段不行了还得继续循环判断下一段
            if (isDivide == false)
                continue;
            //处理消除完球队列发生分裂的几种情况
            //如果是第一个球不为空，此时有两种情况
            //情况一：最后一个球不为空，说明消除发生在中间，此时最后一个球所处的段要产生新队列
            //情况二：最后一个球为空，那么只是相当于原队列变短了，第一个球也没变，所以无需操作
            if(headBall != null)
            {
                targetBallSegementList[j] = headBall;//将消除的头球设置为当前段的第一个球
                //此时如果原本的最后一个球不为空，说明它所处这一段将成为新的一个队列
                if(lastBall != null)
                {
                    //通过最后一个球找到它所处本段的第一个球，插入到列表中
                    //这里用Insert而不是Add，Insert可以中间插入，但Add只能在尾部插入
                    targetBallSegementList.Insert(j + 1, lastBall.HeadBall);
                }
            }
            //若第一个球为空，说明消除发生在头部，此时有两种情况
            //情况一：最后一个球不为空，那么相当于原列表变短了，但第一个球变了，需要重新知道头部球
            //情况二：最后一个球也空了，那么相当于整个队列都消没了，直接移除
            else
            {
                if(lastBall != null)
                {
                    targetBallSegementList[j] = lastBall.HeadBall;//通过消除的尾球找到其第一个球设置头部球
                }
                else
                {
                    targetBallSegementList.RemoveAt(j);
                }
            }
            //处理同色合并
            TargetBall connectBallA = null;//上一段的连接球
            //先找上一段的连接球
            //如果头部球不为空，那么这一段的尾部球即使连接球，用于连接下一段
            if (headBall != null)
                connectBallA = headBall.LastBall;
            //如果头部球为空，那么这一段就全消除掉了，说明要找到它上一段的尾部球作为连接球
            //注意防止越界
            else if (j > 0)
                connectBallA = targetBallSegementList[j - 1].LastBall;
            //当上一段的连接球不为空的时候，这个时候就要找到下一段的连接球用于连接
            //思路和找上一段连接球一样，只是方向反过来
            if (connectBallA != null)
            {
                TargetBall connectBallB = null;//下一段的连接球
                if (lastBall != null)
                    connectBallB = lastBall.HeadBall;
                else if (j + 1 <= targetBallSegementList.Count - 1)
                    connectBallB = targetBallSegementList[j + 1].HeadBall;
                //如果两个连接球的颜色一样，就连起来
                if(connectBallB != null && connectBallB.ballType == connectBallA.ballType)
                {
                    connectBallB.connectTarget = connectBallA;//赋值它的上一段连接球
                    connectBallList.Add(connectBallB);//添加到列表
                }

            }
        }
    }
    //断链回退处理，位置关系
    private void ExecuteBack()
    {
        int k = connectBallList.Count;
        while( k > 0)
        {
            k--;
            TargetBall ball = connectBallList[k];
            //安全校验,两个连接球任意有一个被隐藏了（消除）就直接进行下一个循环
            if(ball.gameObject.activeSelf == false || ball.connectTarget.gameObject.activeSelf == false)
            {
                connectBallList.RemoveAt(k);
                continue;
            }
            ball.position -= Time.deltaTime * 10;//进度更新花费时间
            UPdateConnectBallPosition(ball);//调用方法使所有球向后移动
            //直到回到上一个连接点的位置算连接完毕
            if(ball.position <= ball.connectTarget.position + 1)
            {
                insertList.Add(ball);//这是为了处理连接后还满足消除条件时的情况
                connectBallList.RemoveAt(k);
            }
        }
    }
    //更新下一段的球的所有移动进度
    //处理整段球移动的思路在此，用一个头部球来判定移动多少，然后剩下的依次遍历移动即可
    private void UPdateConnectBallPosition(TargetBall ball)
    {
        while(ball != null)
        {
            //如果一个球的后继不为空，就让它的移动进度往后更新，依次迭代
            if (ball.NextBall != null)
                ball.NextBall.position = ball.position + 1;
            ball = ball.NextBall;
        }
    }
    //断链后的连接处理，数据逻辑关系
    private void ExecuteConnection()
    {
        int i = targetBallSegementList.Count;
        //因为要处理两段，所以要大于1，避免负数
        while(i > 1)
        {
            i--;
            TargetBall preSegment = targetBallSegementList[i - 1];
            TargetBall nextSegment = targetBallSegementList[i];
            TargetBall preTail = preSegment.LastBall;//上一段的最后一个球
            //上一段的最后一个球位于下一段球的下一个移动进程时说明两段连一起了
            //在建立联系的时候要细心点，自己就犯了一个错，之前把preTail用preSegment来代替了，一个尾部而另一个是头部
            if(preTail.position >= nextSegment.position - 1)
            {
                nextSegment.position = preTail.position + 1;//更新下一段球的移动进程
                UPdateConnectBallPosition(nextSegment);//下一段整体移动
                //建立前后关系
                nextSegment.PreBall = preTail;
                preTail.NextBall = nextSegment;
                targetBallSegementList.RemoveAt(i);//由于二合一了，所以将下一段移除
            }
        }
    }
    //处理回退
    private void BallBack()
    {
        int i = targetBallSegementList.Count;
        if (i <= 0) return;
        TargetBall ball = targetBallSegementList[i - 1].LastBall;
        ball.position -= Time.deltaTime * 3;//自己回退
        //带动它前面的球回退
        while(ball.PreBall != null)
        {
            if (ball.PreBall.position > ball.position - 1)
                ball.PreBall.position = ball.position - 1;
            ball = ball.PreBall;
            //接下来处理回到出洞口的情况
            if (ball.position < 0)
            {
                targetBallSegementList[i - 1] = ball.NextBall;//赋值出洞口处的球
                ball.NextBall.PreBall = null;//它的前驱在洞口内了，要回收，所以要把关系置空
                //循环回收
                while (ball != null)
                {
                    targetBallPool.ballAmount--;
                    ball.Recall();
                    ball = ball.PreBall;
                }
                break;
            }
        }
    }
    //失败重生后的状态处理
    public void BackState()
    {
        gameState = GameState.Game;
        moveState = MoveState.Back;
        ScheduleOnce.Start(this, () =>
         {
             moveState = MoveState.Forward;
         }, 5f);
    }
}
