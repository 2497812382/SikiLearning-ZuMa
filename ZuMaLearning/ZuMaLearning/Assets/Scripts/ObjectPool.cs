using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//这个类直接传递一个泛型参数，依据需要生成对象的类型而改变。
public class ObjectPool<T>
{
    public int ballAmount;//每关生成的球数
    private List<T> poolList = new List<T>();//对象池列表
    private Func<T> func;//委托
    //对象池实例化
    //传递委托事件和生成对象的数量，赋值委托并调用实例化方法
    public ObjectPool(Func<T> func, int count)
    {
        this.func = func;
        InstanceObject(count);
    }

    //实例化指定数量的对象
    private void InstanceObject(int count)
    {
        for(int i = 0; i < count; i++)
        {
            poolList.Add(func());//注意是把委托添加到列表中
        }
    }

    //从对象池里获取对象
    public T GetObject()
    {
        //for (int i = poolList.Count; i > 0; i--)
        //{
        //    T t = poolList[i];
        //    poolList.RemoveAt(i);
        //    return t;
        //}
        int i = poolList.Count;
        while(i-- > 0)
        {
            T t = poolList[i];
            poolList.RemoveAt(i);
            return t;
        }
        //这里是为了防止对象池空了，此时再给它2个对象然后调用自身。
        InstanceObject(2);
        return GetObject();
    }
    //往对象池里放对象
    public void AddObject(T t)
    {
        poolList.Add(t);
    }
}
