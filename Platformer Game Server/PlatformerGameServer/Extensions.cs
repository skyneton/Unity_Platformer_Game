using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PlatformerGameServer
{
    public static class Extensions
    {
        public static void Remove<T>(this ConcurrentBag<T> data, T target)
        {
            Queue<T> removeQueue = new Queue<T>();
            while(!data.IsEmpty)
            {
                T item;
                if (data.TryTake(out item) && item.Equals(target))
                    break;
                
                removeQueue.Enqueue(item);
            }
            
            foreach (var item in removeQueue)
            {
                data.Add(item);
            }
        }
    }
}