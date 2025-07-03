using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PMEditor.Util;

public class ObjectPool<T> where T : class
{
    private readonly ConcurrentQueue<T> availableObjects;
    private readonly HashSet<T> leasedObjects;
    private readonly Func<T> objectGenerator;
    private readonly object @lock = new();

    /// <summary>
    /// 初始化对象池
    /// </summary>
    /// <param name="objectGenerator">对象生成函数</param>
    /// <param name="initialCount">初始对象数量</param>
    public ObjectPool(Func<T> objectGenerator, int initialCount = 0)
    {
        this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        availableObjects = new ConcurrentQueue<T>();
        leasedObjects = new HashSet<T>();

        for (int i = 0; i < initialCount; i++)
        {
            availableObjects.Enqueue(objectGenerator());
        }
    }

    /// <summary>
    /// 从池中获取一个对象
    /// </summary>
    /// <returns>池中的对象</returns>
    public T Get()
    {
        lock (@lock)
        {
            if (availableObjects.TryDequeue(out var item))
            {
                leasedObjects.Add(item);
                return item;
            }
            
            // 如果没有可用对象，创建一个新的
            item = objectGenerator();
            leasedObjects.Add(item);
            return item;
        }
    }

    /// <summary>
    /// 将对象释放回池中
    /// </summary>
    /// <param name="item">要释放的对象</param>
    public void Release(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        lock (@lock)
        {
            if (!leasedObjects.Contains(item))
            {
                throw new InvalidOperationException("对象未被此池管理或已被释放");
            }

            leasedObjects.Remove(item);
            availableObjects.Enqueue(item);
        }
    }

    /// <summary>
    /// 获取池中可用对象的数量
    /// </summary>
    public int AvailableCount => availableObjects.Count;

    /// <summary>
    /// 获取当前被租借的对象数量
    /// </summary>
    public int LeasedCount
    {
        get
        {
            lock (@lock)
            {
                return leasedObjects.Count;
            }
        }
    }
}