using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{
    public static class Walker
    {

        public static void DepthFirstWalk(object start, Func<object, List<object>> stepFunction, Action<object> visitorFunction)
        {
            List<object> walkedList = new List<object>();

            Stack q = new Stack();
            q.Push(start);
            while (q.Count > 0)
            {
                var current = q.Pop();
                if (current == null)
                    continue;

                //have we visited already?
                if (walkedList.Contains(current))
                    continue;

                //record visit
                walkedList.Add(current);

                //get next to process
                var next = stepFunction(current);

                //enqueue
                if (next != null)
                    foreach (var each in next)
                        q.Push(each);

                visitorFunction(current);
            }
        }

        public static void BreadthFirstWalk(object start, Func<object, List<object>> stepFunction, Action<object> visitorFunction)
        {
            List<object> walkedList = new List<object>();

            Queue q = new Queue();
            q.Enqueue(start);
            while (q.Count > 0)
            {
                var current = q.Dequeue();
                if (current == null)
                    continue;

                //have we visited already?
                if (walkedList.Contains(current))
                    continue;

                //record visit
                walkedList.Add(current);

                //get next to process
                var next = stepFunction(current);

                //enqueue
                if (next != null)
                    foreach (var each in next)
                        q.Enqueue(each);

                visitorFunction(current);
            }
        }

    
    }
}
