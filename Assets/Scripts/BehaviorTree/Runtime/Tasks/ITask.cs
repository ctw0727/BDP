using System.Collections.Generic;
using BS.BehaviorTrees.Trees;
using UnityEngine;

namespace BS.BehaviorTrees.Tasks
{
    public interface ITask
    {
        /// <summary>
        /// 디버깅을 위해 사용 목적을 적어줄 이름
        /// </summary>
        string Name { get; set; }

        bool Enabled { get; set; }
        GameObject Owner { get; set; }
        IBehaviorTree ParentTree { get; set; }
        List<ITask> Children { get; }
        TaskStatus LastStatus { get; }

        /// <summary>
        /// tick 마다 실행될 업데이트
        /// </summary>
        TaskStatus Update();

        /// <summary>
        /// task가 끝날 때 실행시킬 작업
        /// </summary>
        void End();

        /// <summary>
        /// task를 초기 상태로 되돌림
        /// </summary>
        void Reset();
    }
}