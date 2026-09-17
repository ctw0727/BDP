using System.Collections.Generic;
using BS.BehaviorTrees.TaskParents;
using BS.BehaviorTrees.Tasks;
using BS.BehaviorTrees.Trees;
using UnityEngine;

namespace BS.BehaviorTrees.Decorators
{
    public abstract class DecoratorBase : GenericTaskBase, ITaskParent
    {
        public List<ITask> Children { get; } = new List<ITask>();

        public string Name { get; set; }

        public bool Enabled { get; set; } = true;

        public GameObject Owner { get; set; }
        public IBehaviorTree ParentTree { get; set; }
        public TaskStatus LastStatus { get; private set; }

        public ITask Child => Children.Count > 0 ? Children[0] : null;

        public override TaskStatus Update()
        {
            base.Update();

            if(Child == null)
            {
                if(Application.isPlaying) Debug.LogWarning($"Decorator {Name} 의 자식이 없습니다. failure를 반환합니다. 고치시길 바랍니다.");
                return TaskStatus.Failure;
            }

            var status = OnUpdate();
            LastStatus = status;

            return status;
        }

        protected abstract TaskStatus OnUpdate();

        public void End()
        {
            Child.End();
        }

        public void Reset() {}

        public ITaskParent AddChild(ITask child)
        {
            if(Child == null) Children.Add(child);
            else Debug.LogWarning($"Decorator {Name}은 하나의 자식만 가질 수 있습니다. 자식을 추가하는 것을 무시합니다.");

            return this;
        }
    }
}