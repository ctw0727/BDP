using UnityEngine;

namespace BS.BehaviorTrees.Tasks.Actions
{
    public class TimeMonitor : ITimeMonitor
    {
        public float DeltaTime => Time.deltaTime;
    }
}