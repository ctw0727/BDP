using System.Collections.Generic;
using BS.BehaviorTrees.Trees;
using UnityEngine;

namespace BS.BehaviorTrees.Tasks
{
    public abstract class TaskBase : GenericTaskBase, ITask
    {
        private bool _init;
        private bool _start;
        private bool _exit;
        private int _lastTickCount;
        private bool _active;

        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public GameObject Owner { get; set; }
        public IBehaviorTree ParentTree { get; set; }

        public List<ITask> Children { get; } = null;
        public TaskStatus  LastStatus { get; private set; }

        public override TaskStatus Update()
        {
            base.Update();
            UpdateTicks();

            if(!_init)
            {
                Init();
                _init = true;
            }

            if(!_start)
            {
                Start();
                _start = true;
                _exit = true;
            }

            var status = GetUpdate();
            LastStatus = status;


            if(status != TaskStatus.Continue)
            {
                if(_active) ParentTree?.RemoveTask(this);
                Exit();
            }
            else if(!_active)
            {
                ParentTree?.AddTask(this);
                _active = true;
            }

            return status;
        }

        private void UpdateTicks()
        {
            if(ParentTree == null) return;

            if(_lastTickCount != ParentTree.TickCount) Reset();

            _lastTickCount = ParentTree.TickCount;
        }

        public void Reset()
        {
            _active = false;
            _start = false;
            _exit = false;
        }

        public void End()
        {
            Exit();
        }

        protected virtual TaskStatus GetUpdate()
        {
            return TaskStatus.Failure;
        }

        private void Init()
        {
            OnInit();
        }

        /// <summary>
        /// 노드가 맨처음으로 실행될 때 또는 reset 이후에
        /// <summary>
        protected virtual void OnInit()
        {

        }

        private void Start()
        {
            OnStart();
        }

        /// <summary>
        /// 노드가 실행될 때 마다 실행
        /// </summary>
        protected virtual void OnStart()
        {

        }

        private void Exit()
        {
            if(_exit)
            {
                OnExit();
            }

            Reset();
        }

        /// <summary>
        /// 노드의 작업이 끝날 때 마다
        /// </summary>
        protected virtual void OnExit()
        {

        }
    }
}