using BS.BehaviorTrees.Tasks;

namespace BS.BehaviorTrees.TaskParents
{
    public class TaskRoot : TaskParentBase
    {
        public override string Name { get; set; } = "Root";
        protected override int MaxChildren { get; } = 1;

        protected override TaskStatus OnUpdate()
        {
            if(Children.Count == 0) return TaskStatus.Success;

            var child = Children[0];
            return child.Update();
        }

        public override void End()
        {
            
        }
    }
}