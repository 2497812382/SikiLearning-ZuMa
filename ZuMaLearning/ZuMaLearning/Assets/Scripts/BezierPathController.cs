using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class BezierPathController : MonoBehaviour
{
    public GameObject targetBallPrefab;//目标球预制体，后续要改成数组，因为要放多个。
    public int segmentPerCurve;
    public List<GameObject> ControllerPointList = new List<GameObject>();//存放控制点的列表，控制点类型为GO
    public List<Vector3> TargetBallPointList = new List<Vector3>();//目标球生成位置的列表。

    public bool isShowDrawing = true;

    private void Awake()
    {
        //实例化目标球预制体
        foreach(var item in TargetBallPointList)
        {
           GameObject targetBall = Instantiate(targetBallPrefab, GameObject.Find("TargetBalls").transform);
           targetBall.transform.position = item;
        }
    }
    //调用内置的OnDrawGizmos方法，Gizmos类型的图案只会在场景视图中显示，而Game视图中不显示，非常适合画路径。
    private void OnDrawGizmos()
    {
        //这里要清空列表，不然会无限添加。
        ControllerPointList.Clear();
        //遍历子物体，将它们的Transform添加到列表中
        foreach (Transform item in transform)
        {
            ControllerPointList.Add(item.gameObject);
        }
        //再从控制点列表中获取点的坐标用于后续的计算
        //列表的Select方法需要System.Linq库。
        //注意这种取属性的写法，另外记得要在最后转化成List类型。
        List<Vector3> pointPos = ControllerPointList.Select(point => point.transform.position).ToList();
        var pointsOnCurve = GetDrawPoint(pointPos, segmentPerCurve);

        //实际上曲线上的点不需要全部显示，只需要显示会产生目标球的点即可
        Vector3 startPoint = pointsOnCurve[0];//赋值起始点
        TargetBallPointList.Clear();
        TargetBallPointList.Add(startPoint);//添加到目标球列表中
        for (int k = 0; k < pointsOnCurve.Count; k++)
        {
            //当两个曲线点距离大于某个值时，说明它们可以无缝摆放上目标球。
            //替换起始点，并加入目标球列表。
            if (Vector3.Distance(startPoint, pointsOnCurve[k]) >= 0.42f)
            {
                startPoint = pointsOnCurve[k];
                TargetBallPointList.Add(startPoint);
            }
        }
        //控制球的显示
        foreach(var item in ControllerPointList)
        {
            item.GetComponent<MeshRenderer>().enabled = isShowDrawing;
        }
        //控制绘制线的显示
        if (isShowDrawing == false) return;

        Gizmos.color = Color.red;
        //绘制曲线上的点
        foreach(var item in TargetBallPointList)
        {
            Gizmos.DrawSphere(item, 0.21f);
        }
        Gizmos.color = Color.blue;
        //绘制控制点之间的线段
        for(int i = 0; i < pointPos.Count - 1; i++)
        {
            Gizmos.DrawLine(pointPos[i], pointPos[i + 1]);
        }
        //绘制曲线上的点之间的连线
        Gizmos.color = Color.yellow;
        for(int j = 0; j < pointsOnCurve.Count - 1; j++)
        {
            Gizmos.DrawLine(pointsOnCurve[j], pointsOnCurve[j + 1]);
        }
    }
    //获取曲线绘制点并调用公式进行计算
    //传递的两个参数分别为控制点的列表和每个曲线分成多少段
    //多少段用来控制参数t的改变
    public List<Vector3> GetDrawPoint(List<Vector3> controlPoint, int segmentPerCurve)
    {
        List<Vector3> pointOnCurve = new List<Vector3>();//曲线上的点的列表
        //从列表中取出用于计算的4个点。
        //注意条件语句和累加值，一是不要越界，二是要让上一个组合中的最后一个点成为下一个组合的第一个点，以保证曲线的连续。
        for(int i = 0; i < controlPoint.Count - 3; i+=3)
        {
            //var类型是根据传递的参数类型来决定的。
            var p0 = controlPoint[i];
            var p1 = controlPoint[i+1];
            var p2 = controlPoint[i+2];
            var p3 = controlPoint[i+3];
            for (int j = 0; j < segmentPerCurve; j++)
            {
                float t = j / (float)segmentPerCurve;//控制t的取值，范围为[0,1]
                pointOnCurve.Add(BezierCubicFormula(t, p0, p1, p2, p3));
            }
        }
        return pointOnCurve;
    }
    //贝塞尔三次曲线计算公式
    public Vector3 BezierCubicFormula(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return p0 * Mathf.Pow((1 - t), 3) + 3 * p1 * t * Mathf.Pow((1 - t), 2) + 3 * p2 * 
            Mathf.Pow(t, 2) * (1 - t) + p3 * Mathf.Pow(t, 3);
    }
    //创建并保存地图信息
    public void CreateMapAssets()
    {
        string assetsSavePath = "Assets/Maps/map.asset";//保存路径
        MapConfig mapConfig = new MapConfig();//创建地图信息配置类
        //把目标球生成点添加到mapConfig的列表中
        foreach(var item in TargetBallPointList)
        {
            mapConfig.TargetBallPointList.Add(item);
        }
        //在指定的路径下创建对象的Asset。
        //需要引入UnityEditor库。
        AssetDatabase.CreateAsset(mapConfig, assetsSavePath);
        AssetDatabase.SaveAssets();
    }
    //监视面板拓展工具,也需要引入UnityEditor库。
    [CustomEditor(typeof(BezierPathController))]
    //面板编辑器类要继承自Editor类。
    public class BezierEditor : Editor
    {
        //重写监视面板UI方法
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Generate Map"))
            {
                //这里把按钮的对象强转为控制器类，这样才能调用它里面的创建并保存地图信息方法
                (target as BezierPathController).CreateMapAssets();
            }
        }
    }
}
