using UnityEngine;

public class ArrowPointCollider : MonoBehaviour
{
    public EnemyCutDirection.ArrowPointType pointType;

    private EnemyCutDirection enemyCutDirection;

    private void Start()
    {
        enemyCutDirection = GetComponentInParent<EnemyCutDirection>();

        if (enemyCutDirection == null)
        {
            Debug.LogWarning("EnemyCutDirection not found in parent of " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Katana"))
        {
            return;
        }

        if (enemyCutDirection != null)
        {
            enemyCutDirection.RegisterArrowPointHit(pointType);
        }
    }
}