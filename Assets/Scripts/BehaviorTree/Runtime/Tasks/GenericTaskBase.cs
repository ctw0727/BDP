namespace BS.BehaviorTrees.Tasks
{
    public abstract class GenericTaskBase
    {
        public virtual TaskStatus Update()
        {
            return TaskStatus.Success;
        }
    }
}