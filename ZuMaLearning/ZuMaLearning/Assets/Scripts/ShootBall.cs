using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    public BallType ballType;
    //初始化射击球样式和控制显示
    public void Init(BallType type, Sprite sp, Transform tr)
    {
        ballType = type;
        //GetComponent<SpriteRenderer>().sprite = GameManager._instance.GetSpriteByBallType(type);//耦合性较高
        GetComponent<SpriteRenderer>().sprite = sp;
        transform.localPosition = tr.position;
        transform.rotation = tr.rotation;
        gameObject.SetActive(true);
    }
    public void Move()
    {
        //发射球
        transform.localPosition += transform.up * Time.deltaTime * 8;
    }
    //判断发射球是否超出边界
    public bool IsOutOfBoundary()
    {
        if (transform.localPosition.x > 3 || transform.localPosition.x < -3 ||
            transform.localPosition.y > 5 || transform.localPosition.y < -5)
            return true;
        else
            return false;
    }
    //判断发射球自己和目标球之间的距离是否满足插入条件
    public bool IsInsert(Vector3 targetPos, float distance)
    {
        return Vector3.Distance(transform.position, targetPos) < distance;
    }
}
