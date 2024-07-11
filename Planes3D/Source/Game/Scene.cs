using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Planes3D
{
    public sealed class Scene
    {
        private class Task
        {
            public float Time;
            public Action<float> Action;
        }

        private Stopwatch timeSinceLoad;
        private List<Task> tasks;
        private List<GameObject> objectList;
        private List<Task> taskRemovalList;
        private List<GameObject> objectRemovalList;

        public float TimeSinceLoad;

        public Scene()
        {
            timeSinceLoad = new Stopwatch();
            timeSinceLoad.Start();

            tasks = new List<Task>();
            objectList = new List<GameObject>();

            taskRemovalList = new List<Task>();
            objectRemovalList = new List<GameObject>();
        }

        public void Add(GameObject obj)
        {
            if (!objectList.Contains(obj))
                objectList.Add(obj);
        }

        public void Remove(GameObject obj)
        {
            if (objectList.Contains(obj))
                objectRemovalList.Add(obj);
        }

        public void ScheduleTask(float delay, Action<float> action)
        {
            tasks.Add(new Task()
            {
                Time = TimeSinceLoad + delay,
                Action = action
            });
        }

        public void Update()
        {
            foreach (GameObject obj in objectList)
                obj.Update();

            foreach (GameObject obj in objectRemovalList)
                objectList.Remove(obj);

            foreach(Task task in tasks)
            {
                if (task.Time < TimeSinceLoad)
                    task.Action(TimeSinceLoad);

                taskRemovalList.Add(task);
            }

            foreach (Task task in taskRemovalList)
                tasks.Remove(task);

            objectRemovalList.Clear();
            taskRemovalList.Clear();
            TimeSinceLoad = (float)timeSinceLoad.ElapsedMilliseconds / 1000.0f;
        }

        public void Draw()
        {
            foreach (GameObject obj in objectList)
                obj.Draw();
        }
    }
}
