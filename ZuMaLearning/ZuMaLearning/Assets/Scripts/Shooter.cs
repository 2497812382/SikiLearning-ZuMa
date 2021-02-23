using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shooter : MonoBehaviour
{
    private SpriteRenderer shootBall;
    private BallType shootBallType;
    private void Start()
    {
        shootBall = transform.Find("Sprite_Ball").GetComponent<SpriteRenderer>();
        RegenerateShootBall();
    }
    private void Update()
    {
        //如果点到了UI就不执行发生
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        //鼠标左键按下
        if(Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float y = mousePos.y - transform.position.y;
            float x = mousePos.x - transform.position.x;
            //利用反正切函数求出旋转弧度，然后再弧度转角度。
            float rotateAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, rotateAngle-90);
        }
        if (Input.GetMouseButtonUp(0))
        {
            Shoot();
        }
    }

    //刷新发射球的显示
    public void RegenerateShootBall()
    {
        shootBallType = BallStrategy.GenerateShootBallType();//调用生成策略
        shootBall.sprite = GameManager._instance.GetSpriteByBallType(shootBallType);//调用由类型产生sprite
        shootBall.gameObject.SetActive(true);
    }
    //发射
    private void Shoot()
    {
        if (shootBall.gameObject.activeSelf == false)
            return;
        SoundManager.PlayShoot();
        ShootBallManager._instance.Shoot(shootBallType, shootBall.sprite, transform);
        shootBall.gameObject.SetActive(false);
        //Invoke("RegenerateShootBall", 0.2f);//每次发射后都刷新一次，保证球的不同
        //改成封装好的延时执行类
        ScheduleOnce.Start(this, RegenerateShootBall, 0.2f);
    }
}
