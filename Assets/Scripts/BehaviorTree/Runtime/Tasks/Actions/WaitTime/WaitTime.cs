namespace BS.BehaviorTrees.Tasks.Actions
{
    /// <summary>
    /// 시간이 다될때까지 running을 반환
    /// </summary>
    public class WaitTime : ActionBase
    {
        private ITimeMonitor _timeMonitor;
        private float _timePassed;

        public float time = 1;

        public WaitTime(ITimeMonitor timeMonitor)
        {
            _timeMonitor = timeMonitor;
        }

        protected override void OnStart()
        {
            _timePassed = 0;
        }

        protected override TaskStatus OnUpdate()
        {
            _timePassed += _timeMonitor.DeltaTime;

            if(_timePassed < time) return TaskStatus.Continue;

            return TaskStatus.Success;
        }
    }
}