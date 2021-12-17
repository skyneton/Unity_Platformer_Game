using System.Collections.Concurrent;
using System.Threading;

namespace PlatformerGameServer.Utils
{
    public class ThreadFactory
    {
        private static readonly ConcurrentBag<Thread> Threads = new();

        public static Thread LaunchThread(Thread thread, bool setName = true)
        {
            thread.Start();
            
            if (Threads == null) return thread;
            
            if(setName)
                thread.Name = "Thread-" + (Threads.Count - 1);
            
            Threads.Add(thread);

            return thread;
        }

        public static void KillAll()
        {
            if (Threads == null) return;
            
            foreach (var thread in Threads)
            {
                if(thread.IsAlive)
                    thread.Interrupt();
            }
        }
    }
}