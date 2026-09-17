using BS.BehaviorTrees.Tasks;

namespace BS.BehaviorTrees.TaskParents
{
    public interface ITaskParent : ITask{
        ITaskParent AddChild(ITask child);
    }
}
