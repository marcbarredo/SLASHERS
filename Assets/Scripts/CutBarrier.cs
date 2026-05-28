using UnityEngine;

public class CutBarrier : MonoBehaviour
{
    [SerializeField] private int barrierNumber = 1;

    private TwoBarrierCutEnemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<TwoBarrierCutEnemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy == null)
            return;

        enemy.BarrierTouched(barrierNumber, other);
    }
}