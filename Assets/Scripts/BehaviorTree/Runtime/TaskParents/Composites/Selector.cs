using BS.BehaviorTrees.Tasks;

/// <summary>
/// Selector는 자식 노드가 Failure가 아닐 때까지 순회합니다.
/// 즉 한 가지만 선택하여 실행되게 합니다.
/// <summary>
namespace BS.BehaviorTrees.TaskParents.Composites
{
    public class Selector : CompositeBase
    {
        protected override TaskStatus OnUpdate()
        {
            for(var i = ChildIndex; i < Children.Count; i++)
            {
                var child = Children[ChildIndex];

                switch(child.Update())
                {
                    case TaskStatus.Success:
                        return TaskStatus.Success;
                    case TaskStatus.Continue:
                        return TaskStatus.Continue;
                }

                ChildIndex++;
            }

            return TaskStatus.Failure;
        }
    }
}