  using BS.BehaviorTrees.Tasks;

namespace BS.BehaviorTrees.Decorators {
    public class ReturnSuccess : DecoratorBase {
        protected override TaskStatus OnUpdate () {
            var status = Child.Update();
            if (status == TaskStatus.Continue) {
                return status;
            }

            return TaskStatus.Success;
        }
    }
}