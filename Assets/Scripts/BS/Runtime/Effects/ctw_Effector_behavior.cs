using UnityEngine;

public class ctw_Effector_behavior : MonoBehaviour
{
    public GameObject EffectPrefab;

    public GameObject[] Effect = new GameObject[100];
    public int EffectPool;

    public void Effect_Run(float time, Vector2 position, Vector2 velocity, int count)
    {
        for (int i = 0; i < count; i++)
        {
            ctw_BlockEffect_behavior effect = Rent();
            if (effect == null)
                continue;

            Vector2 jitter = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            effect.Play(position, velocity + jitter, time);
        }
    }

    ctw_BlockEffect_behavior Rent()
    {
        for (int i = 0; i < EffectPool; i++)
        {
            if (Effect[i] == null)
                continue;

            ctw_BlockEffect_behavior effect = Effect[i].GetComponent<ctw_BlockEffect_behavior>();
            if (effect != null && !effect.OnWork)
                return effect;
        }

        if (EffectPrefab == null || EffectPool >= Effect.Length)
            return null;

        GameObject created = Instantiate(EffectPrefab, transform);
        Effect[EffectPool] = created;
        EffectPool++;
        return created.GetComponent<ctw_BlockEffect_behavior>();
    }
}
