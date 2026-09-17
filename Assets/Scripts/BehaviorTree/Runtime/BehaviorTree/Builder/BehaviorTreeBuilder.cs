using System;
using System.Collections.Generic;
using BS.BehaviorTrees.Decorators;
using BS.BehaviorTrees.TaskParents;
using BS.BehaviorTrees.TaskParents.Composites;
using BS.BehaviorTrees.Tasks;
using BS.BehaviorTrees.Tasks.Actions;
using UnityEngine;

namespace BS.BehaviorTrees.Trees
{
    public class BehaviorTreeBuilder
    {
        private readonly BehaviorTree _tree;
        private readonly List<ITaskParent> _pointers = new List<ITaskParent>();

        private ITaskParent PointerCurrent
        {
            get
            {
                if(_pointers.Count == 0) return null;
                return _pointers[_pointers.Count - 1];
            }
        }

        public BehaviorTreeBuilder(GameObject owner)
        {
            _tree = new BehaviorTree(owner);
            _pointers.Add(_tree.Root);
        }

        public BehaviorTreeBuilder Name(string name)
        {
            _tree.Name = name;
            return this;
        }

        public BehaviorTreeBuilder ParentTask<T>(string name) where T : ITaskParent, new()
        {
            var parent = new T { Name = name };

            return AddNodeWithPointer(parent);
        }

        public BehaviorTreeBuilder Decorator(string name, Func<ITask, TaskStatus> logic)
        {
            var decorator = new DecoratorGeneric {
                updateLogic = logic,
                Name = name
            };

            return AddNodeWithPointer(decorator);
        }

        public BehaviorTreeBuilder Decorator(Func<ITask, TaskStatus> logic)
        {
            return Decorator("decorator", logic);
        }

        public BehaviorTreeBuilder AddNodeWithPointer(ITaskParent task)
        {
            AddNode(task);
            _pointers.Add(task);

            return this;
        }

        public BehaviorTreeBuilder Inverter(string name = "inverter")
        {
            return ParentTask<Inverter>(name);
        }

        public BehaviorTreeBuilder ReturnSuccess (string name = "return success") 
        {
            return ParentTask<ReturnSuccess>(name);
        }
        
        public BehaviorTreeBuilder ReturnFailure (string name = "return failure") 
        {
            return ParentTask<ReturnFailure>(name);
        }

        public BehaviorTreeBuilder RepeatUntilSuccess (string name = "repeat until success") {
            return ParentTask<RepeatUntilSuccess>(name);
        }

        public BehaviorTreeBuilder RepeatUntilFailure (string name = "repeat until failure") {
            return ParentTask<RepeatUntilFailure>(name);
        }

        public BehaviorTreeBuilder RepeatForever (string name = "repeat forever") {
            return ParentTask<RepeatForever>(name);
        }

        public BehaviorTreeBuilder Sequence(string name = "sequence")
        {
            return ParentTask<Sequence>(name);
        }

        public BehaviorTreeBuilder Selector(string name = "selector")
        {
            return ParentTask<Selector>(name);
        }

        public BehaviorTreeBuilder Parallel(string name = "parallel")
        {
            return ParentTask<Parallel>(name);
        }

        public BehaviorTreeBuilder Do(string name, Func<TaskStatus> action)
        {
            return AddNode(new ActionGeneric {
                Name = name, updateLogic = action
            });
        }

        public BehaviorTreeBuilder Do(Func<TaskStatus> action)
        {
            return Do("action", action);
        }
        
        public BehaviorTreeBuilder WaitTime (string name, float time = 1f) {
            return AddNode(new WaitTime(new TimeMonitor()) {
                Name = name,
                time = time
            });
        }

        public BehaviorTreeBuilder WaitTime (float time = 1f) {
            return WaitTime("Wait Time", time);
        }

        public BehaviorTreeBuilder Condition (string name, Func<bool> action) {
            return AddNode(new ConditionGeneric {
                Name = name,
                updateLogic = action
            });
        }
        
        public BehaviorTreeBuilder Condition (Func<bool> action) {
            return Condition("condition", action);
        }

        public BehaviorTreeBuilder AddNode(ITask node)
        {
            _tree.AddNode(PointerCurrent, node);
            return this;
        }

        public BehaviorTreeBuilder Spilce(BehaviorTree tree)
        {
            _tree.Splice(PointerCurrent, tree);

            return this;
        }

        public BehaviorTreeBuilder End () 
        {
            _pointers.RemoveAt(_pointers.Count - 1);
            
            return this;
        }

        public BehaviorTree Build()
        {
            return _tree;
        }
    }
}