using System.Collections.Generic;
using BS.BehaviorTrees.TaskParents;
using BS.BehaviorTrees.Tasks;
using UnityEngine;

namespace BS.BehaviorTrees.Trees{
    public interface IBehaviorTree{
        string Name { get; }
        TaskRoot Root { get; }
        int TickCount { get; }

        void AddTask(ITask task);
        void RemoveTask(ITask task);
    }

    [System.Serializable]
    public class BehaviorTree : IBehaviorTree{
        private readonly GameObject _owner;
        private readonly List<ITask> _tasks = new List<ITask>();

        public int TickCount { get; private set; }

        public string Name { get; set; }
        public TaskRoot Root { get; } = new TaskRoot();
        public IReadOnlyList<ITask> ActiveTasks => _tasks;
        
        public BehaviorTree(GameObject owner)
        {
            _owner = owner;
            SyncNodes(Root);
        }

        public TaskStatus Tick()
        {
            var status = Root.Update();
            if(status != TaskStatus.Continue) Reset();

            return status;
        }

        public void Reset()
        {
            foreach(var task in _tasks)
            {
                task.End();
            }

            _tasks.Clear();
            TickCount++;
        }
        public void AddNode(ITaskParent parent, ITask child)
        {
            parent.AddChild(child);
            child.ParentTree = this;
            child.Owner = _owner;
        }

        public void Splice(ITaskParent parent, BehaviorTree tree)
        {
            parent.AddChild(tree.Root);

            SyncNodes(tree.Root);
        }

        private void SyncNodes(ITaskParent taskParent)
        {
            taskParent.Owner = _owner;
            taskParent.ParentTree = this;

            foreach(var child in taskParent.Children)
            {
                child.Owner = _owner;
                child.ParentTree = this;

                var parent = child as ITaskParent;
                if(parent != null)
                {
                    SyncNodes(parent);
                }
            }
        }

        public void AddTask(ITask task)
        {
            _tasks.Add(task);
        }

        public void RemoveTask(ITask task)
        {
            _tasks.Remove(task);
        }
    }
}
