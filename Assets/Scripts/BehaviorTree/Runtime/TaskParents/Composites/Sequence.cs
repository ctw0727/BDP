using BS.BehaviorTrees.Tasks;

/// <summary>
/// Sequece는 자식 노드가 Success가 아닐 때까지 순회합니다.
/// 따라서 순차적으로 실행되게 됩니다.
/// <summary>
namespace BS.BehaviorTrees.TaskParents.Composites
{
    public class Sequence : CompositeBase
    {
        protected override TaskStatus OnUpdate()
        {
            for(var i = ChildIndex; i < Children.Count; i++)
            {
                var child = Children[ChildIndex];
                var status = child.Update();
    
                if(status != TaskStatus.Success) return status;

                ChildIndex++;
            }

            return TaskStatus.Success;
        }
    }
}