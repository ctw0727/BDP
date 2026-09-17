using BS.BehaviorTrees.Tasks;

namespace BS.BehaviorTrees.Decorators
{
    public class RepeatForever : DecoratorBase
    {
        protected override TaskStatus OnUpdate()
        {
            Child.Update();
            return TaskStatus.Continue;
        }
    }
}