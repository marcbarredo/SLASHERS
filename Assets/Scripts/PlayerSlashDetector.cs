using UnityEngine;

/// <summary>
/// Attach this to the tracked controller / player object that moves with PlayerMovement.
/// It detects fast cuts on the XZ floor plane, shows a TrailRenderer only when moving fast,
/// and kills enemies only when the slash is close to the enemy and parallel to its arrow.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerSlashDetector : MonoBehaviour
{
    [Header("Slash speed")]
    [SerializeField] private float minSlashSpeed = 3.0f;
    [SerializeField] private float minSlashDistance = 0.20f;
    [SerializeField] private float slashCooldown = 0.12f;

    [Header("Direction check")]
    [Tooltip("Allowed angle difference between player slash and enemy arrow direction.")]
    [SerializeField] private float angleTolerance = 25f;

    [Tooltip("True = horizontal accepts both left->right and right->left. This matches the teacher's 'parallel lines' idea.")]
    [SerializeField] private bool acceptOppositeDirection = true;

    [Header("Hit check")]
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float slashWidth = 0.75f;
    [SerializeField] private float overlapExtraRadius = 0.35f;

    [Header("Full cut check")]
    [Tooltip("Minimum length of the slash projected onto the required enemy direction. This prevents just touching the enemy.")]
    [SerializeField] private float minCutLength = 0.75f;

    [Tooltip("If true, the slash must pass from one side of the enemy to the other side along the required cut line.")]
    [SerializeField] private bool mustCrossEnemyCenterLine = true;

    [Header("Visuals")]
    [SerializeField] private TrailRenderer slashTrail;
    [SerializeField] private GameObject cutEffectPrefab;
    [SerializeField] private float destroyCutEffectAfter = 0.5f;

    private Vector3 lastPosition;
    private float lastCutTime;

    private void Awake()
    {
        lastPosition = transform.position;

        if (slashTrail != null)
        {
            slashTrail.emitting = false;
            slashTrail.Clear();
        }
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;

        Vector3 movement = currentPosition - lastPosition;
        movement.y = 0f; // top-down game: only read floor movement

        float distance = movement.magnitude;
        float speed = distance / Mathf.Max(Time.deltaTime, 0.0001f);
        bool isFastSlash = speed >= minSlashSpeed && distance >= minSlashDistance;

        if (slashTrail != null)
            slashTrail.emitting = isFastSlash;

        if (isFastSlash && Time.time >= lastCutTime + slashCooldown)
        {
            Vector3 slashDirection = movement.normalized;
            TryCutEnemies(lastPosition, currentPosition, slashDirection);
        }

        lastPosition = currentPosition;
    }

    private void TryCutEnemies(Vector3 start, Vector3 end, Vector3 slashDirection)
    {
        Vector3 startXZ = new Vector3(start.x, 0f, start.z);
        Vector3 endXZ = new Vector3(end.x, 0f, end.z);
        Vector3 middle = (start + end) * 0.5f;

        float searchRadius = Vector3.Distance(startXZ, endXZ) * 0.5f + slashWidth + overlapExtraRadius;
        Collider[] candidates = Physics.OverlapSphere(middle, searchRadius, enemyLayers);

        foreach (Collider candidate in candidates)
        {
            EnemyCutDirection enemyCut = candidate.GetComponentInParent<EnemyCutDirection>();
            if (enemyCut == null)
                continue;

            Vector3 enemyPos = enemyCut.transform.position;
            float distanceToSlash = DistancePointToSegmentXZ(enemyPos, start, end);

            if (distanceToSlash > slashWidth)
                continue;

            if (!IsSlashDirectionValid(slashDirection, enemyCut.requiredCutDirection))
                continue;

            if (!IsSlashLongEnoughToCutLine(start, end, enemyPos, enemyCut.requiredCutDirection))
                continue;

            CutEnemy(enemyCut.gameObject, enemyPos, slashDirection);
            lastCutTime = Time.time;
        }
    }

    private bool IsSlashDirectionValid(Vector3 slashDirection, EnemyCutDirection.CutDirection requiredDirection)
    {
        Vector3 requiredVector = GetDirectionVector(requiredDirection);

        float dot = Vector3.Dot(slashDirection.normalized, requiredVector.normalized);

        if (acceptOppositeDirection)
            dot = Mathf.Abs(dot);

        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
        return angle <= angleTolerance;
    }

    private Vector3 GetDirectionVector(EnemyCutDirection.CutDirection direction)
    {
        switch (direction)
        {
            case EnemyCutDirection.CutDirection.Horizontal:
                return Vector3.right;

            case EnemyCutDirection.CutDirection.Vertical:
                return Vector3.forward;

            case EnemyCutDirection.CutDirection.DiagonalSlash:
                return new Vector3(1f, 0f, 1f).normalized;

            case EnemyCutDirection.CutDirection.DiagonalBackslash:
                return new Vector3(1f, 0f, -1f).normalized;

            default:
                return Vector3.right;
        }
    }

    private float DistancePointToSegmentXZ(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        Vector2 p = new Vector2(point.x, point.z);
        Vector2 a = new Vector2(segmentStart.x, segmentStart.z);
        Vector2 b = new Vector2(segmentEnd.x, segmentEnd.z);

        Vector2 ab = b - a;
        float abLengthSq = ab.sqrMagnitude;

        if (abLengthSq <= 0.0001f)
            return Vector2.Distance(p, a);

        float t = Vector2.Dot(p - a, ab) / abLengthSq;
        t = Mathf.Clamp01(t);

        Vector2 closest = a + ab * t;
        return Vector2.Distance(p, closest);
    }


    private bool IsSlashLongEnoughToCutLine(Vector3 start, Vector3 end, Vector3 enemyPosition, EnemyCutDirection.CutDirection requiredDirection)
    {
        Vector3 requiredAxis = GetDirectionVector(requiredDirection).normalized;
        requiredAxis.y = 0f;

        Vector3 startOffset = start - enemyPosition;
        Vector3 endOffset = end - enemyPosition;
        startOffset.y = 0f;
        endOffset.y = 0f;

        float startAlongRequiredLine = Vector3.Dot(startOffset, requiredAxis);
        float endAlongRequiredLine = Vector3.Dot(endOffset, requiredAxis);

        float projectedSlashLength = Mathf.Abs(endAlongRequiredLine - startAlongRequiredLine);

        if (projectedSlashLength < minCutLength)
            return false;

        if (!mustCrossEnemyCenterLine)
            return true;

        bool startsAndEndsOnDifferentSides =
            (startAlongRequiredLine <= 0f && endAlongRequiredLine >= 0f) ||
            (startAlongRequiredLine >= 0f && endAlongRequiredLine <= 0f);

        return startsAndEndsOnDifferentSides;
    }

    private void CutEnemy(GameObject enemy, Vector3 position, Vector3 slashDirection)
    {
        if (cutEffectPrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(slashDirection, Vector3.up);
            GameObject effect = Instantiate(cutEffectPrefab, position + Vector3.up * 0.25f, rotation);
            Destroy(effect, destroyCutEffectAfter);
        }

        Destroy(enemy);
    }
}
