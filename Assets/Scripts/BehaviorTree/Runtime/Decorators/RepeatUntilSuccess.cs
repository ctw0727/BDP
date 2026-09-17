using BS.BehaviorTrees.Tasks;

namespace BS.BehaviorTrees.Decorators
{
    public class RepeatUntilSuccess : DecoratorBase
    {
        protected override TaskStatus OnUpdate()
        {
            if(Child.Update() == TaskStatus.Success) return TaskStatus.Success;

            return TaskStatus.Continue;
        }
    }
}