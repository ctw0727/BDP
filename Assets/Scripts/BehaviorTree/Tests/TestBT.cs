using UnityEngine;
using BS.BehaviorTrees.Tasks;
using BS.BehaviorTrees.Trees;

public class TestBT : MonoBehaviour
{
    [SerializeField]
    private BehaviorTree _tree;

    private void Awake()
    {
        _tree = new BehaviorTreeBuilder(gameObject)
                    .Sequence()
                        .Do("Custom Action", () => {
                            Debug.Log("커스텀 액션");
                            return TaskStatus.Success;
                        })
                    .End()
                    .Build();
    }

    private void Update()
    {
        _tree.Tick();
    }
}
