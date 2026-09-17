using System;
using BS.BehaviorTrees.Tasks;

namespace BS.BehaviorTrees.Decorators
{
    public class DecoratorGeneric : DecoratorBase
    {
        public Func<ITask, TaskStatus> updateLogic;

        protected override TaskStatus OnUpdate()
        {
            if(updateLogic != null) return updateLogic(Child);

            return Child.Update();
        }
    }
}