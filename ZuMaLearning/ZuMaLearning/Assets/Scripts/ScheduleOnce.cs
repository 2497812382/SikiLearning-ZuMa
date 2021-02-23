using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduleOnce
{
    private MonoBehaviour mono;
    private IEnumerator enumerator;
    //定义一个静态方法，用来开启协程并返回类本身
    public static ScheduleOnce Start(MonoBehaviour mono, System.Action action, float time)
    {
        return new ScheduleOnce(mono, action, time);
    }
    //停止协程
    public void Stop()
    {
        if(enumerator != null)
        {
            mono.StopCoroutine(enumerator);
            enumerator = null;
        }
    }
    //是否正在执行
    public bool isDoing()
    {
        return enumerator != null;
    }
    //定义类本身，用来调用协程
    private ScheduleOnce(MonoBehaviour mono, System.Action action, float time)
    {
        this.mono = mono;
        enumerator = WaitForSecondsToDO(action, time);
        mono.StartCoroutine(enumerator);
    }
    //延迟time的时间执行action的事件的委托。
   private IEnumerator WaitForSecondsToDO(System.Action action, float time)
    {
        yield return new WaitForSeconds(time);
        action();
        enumerator = null;
    }
}
