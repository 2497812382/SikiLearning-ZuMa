using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapConfig : ScriptableObject
{
    public List<Vector3> TargetBallPointList = new List<Vector3>();//目标球位置列表
    
    //根据目标球传过来的需要移动到的下一个阶段来去目标球列表中取出对应位置的目标球
    public Vector3 GetPosition(float position)
    {
        //先通过取整得到列表下标
        int index = Mathf.FloorToInt(position);
        //再通过队列下标确定具体的位置范围。
        //然后通过计算阶段的小数点部分得出插值的比例，便能准确得出该阶段对应的具体位置
        return Vector3.Lerp(TargetBallPointList[index], TargetBallPointList[index + 1], position - index);
    }
    //赋值终点球的下标（倒数第二个球的位置）
    public void InitLastPoint()
    {
        LastPoint = TargetBallPointList.Count - 2;
    }
    public float LastPoint { get; private set; }//终点球下标
}
