using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerSlashDetector : MonoBehaviour
{
    [Header("Blade Tracking")]
    [Tooltip("Empty object placed around the middle of the katana blade, not the handle.")]
    [SerializeField] private Transform bladePoint;

    [Tooltip("If the blade moves more than this in one frame, ignore it. This prevents teleporting / editor dragging from becoming a fake slash.")]
    [SerializeField] private float maxAllowedFrameMovement = 2.0f;

    [Header("Slash speed")]
    [SerializeField] private float minSlashSpeed = 3.0f;
    [SerializeField] private float minSlashDistance = 0.35f;
    [SerializeField] private float slashCooldown = 0.12f;

    [Header("Direction check")]
    [Tooltip("Allowed angle difference between player slash and enemy arrow direction.")]
    [SerializeField] private float angleTolerance = 25f;

    [Tooltip("True = accepts both directions. Example: left-to-right and right-to-left both count as horizontal.")]
    [SerializeField] private bool acceptOppositeDirection = true;

    [Header("Hit check")]
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float slashWidth = 0.75f;
    [SerializeField] private float overlapExtraRadius = 0.35f;

    [Header("Full cut check")]
    [Tooltip("Minimum length of the slash projected onto the required enemy direction.")]
    [SerializeField] private float minCutLength = 0.75f;

    [Tooltip("If true, the slash must pass from one side of the enemy to the other side.")]
    [SerializeField] private bool mustCrossEnemyCenterLine = true;

    [Header("Visuals")]
    [Tooltip("Put several TrailRenderers along the blade here: base, middle, tip, etc.")]
    [SerializeField] private TrailRenderer[] slashTrails;

    [SerializeField] private GameObject cutEffectPrefab;
    [SerializeField] private float destroyCutEffectAfter = 0.5f;

    [Header("Slash Audio")]
    [SerializeField] private AudioClip slashSound;
    [SerializeField] private float slashVolume = 1f;
    [SerializeField] private float slashSoundCooldown = 0.15f;

    [Header("Enemy Kill Audio")]
    [SerializeField] private AudioClip enemyKillSound;
    [SerializeField] private float enemyKillVolume = 1f;

    private Vector3 lastBladePosition;

    private bool currentlySlashing;
    private Vector3 slashStart;
    private Vector3 slashEnd;

    private float lastCutTime;
    private float lastSlashSoundTime;

    private void Awake()
    {
        if (bladePoint == null)
        {
            Debug.LogError("PlayerSlashDetector needs a Blade Point assigned.");
            enabled = false;
            return;
        }

        ResetSlashTracking();
    }

    private void Update()
    {
        if (bladePoint == null)
            return;

        Vector3 currentBladePosition = bladePoint.position;

        Vector3 movement = currentBladePosition - lastBladePosition;

        // Prevents teleporting / editor dragging from being counted as a huge slash.
        if (movement.magnitude > maxAllowedFrameMovement)
        {
            ResetSlashTracking();
            lastBladePosition = currentBladePosition;
            return;
        }

        movement.y = 0f;

        float distanceThisFrame = movement.magnitude;
        float speed = distanceThisFrame / Mathf.Max(Time.deltaTime, 0.0001f);

        bool bladeIsMovingFast = speed >= minSlashSpeed;

        SetTrailsEmitting(bladeIsMovingFast);

        if (bladeIsMovingFast)
        {
            if (!currentlySlashing)
            {
                currentlySlashing = true;
                slashStart = lastBladePosition;

                PlaySlashSound();
            }

            slashEnd = currentBladePosition;
        }
        else
        {
            if (currentlySlashing)
                FinishSlash();

            currentlySlashing = false;
        }

        lastBladePosition = currentBladePosition;
    }

    private void FinishSlash()
    {
        if (Time.time < lastCutTime + slashCooldown)
            return;

        Vector3 slashMovement = slashEnd - slashStart;
        slashMovement.y = 0f;

        float slashDistance = slashMovement.magnitude;

        if (slashDistance < minSlashDistance)
            return;

        Vector3 slashDirection = slashMovement.normalized;

        bool cutSomething = TryCutEnemies(slashStart, slashEnd, slashDirection);

        if (cutSomething)
            lastCutTime = Time.time;
    }

    private bool TryCutEnemies(Vector3 start, Vector3 end, Vector3 slashDirection)
    {
        bool cutSomething = false;

        Vector3 startXZ = new Vector3(start.x, 0f, start.z);
        Vector3 endXZ = new Vector3(end.x, 0f, end.z);
        Vector3 middle = (start + end) * 0.5f;

        float searchRadius =
            Vector3.Distance(startXZ, endXZ) * 0.5f
            + slashWidth
            + overlapExtraRadius;

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
            cutSomething = true;
        }

        return cutSomething;
    }

    private bool IsSlashDirectionValid(
        Vector3 slashDirection,
        EnemyCutDirection.CutDirection requiredDirection)
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

    private bool IsSlashLongEnoughToCutLine(
        Vector3 start,
        Vector3 end,
        Vector3 enemyPosition,
        EnemyCutDirection.CutDirection requiredDirection)
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
            startAlongRequiredLine <= 0f && endAlongRequiredLine >= 0f ||
            startAlongRequiredLine >= 0f && endAlongRequiredLine <= 0f;

        return startsAndEndsOnDifferentSides;
    }

    private void CutEnemy(GameObject enemy, Vector3 position, Vector3 slashDirection)
    {
        if (cutEffectPrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(slashDirection, Vector3.up);

            GameObject effect = Instantiate(
                cutEffectPrefab,
                position + Vector3.up * 0.25f,
                rotation
            );

            Destroy(effect, destroyCutEffectAfter);
        }

        if (enemyKillSound != null)
        {
            AudioSource.PlayClipAtPoint(
                enemyKillSound,
                position,
                enemyKillVolume
            );
        }

        Destroy(enemy);
    }

    private void PlaySlashSound()
    {
        if (slashSound == null)
            return;

        if (Time.time < lastSlashSoundTime + slashSoundCooldown)
            return;

        AudioSource.PlayClipAtPoint(
            slashSound,
            bladePoint.position,
            slashVolume
        );

        lastSlashSoundTime = Time.time;
    }

    private void SetTrailsEmitting(bool emitting)
    {
        if (slashTrails == null)
            return;

        foreach (TrailRenderer trail in slashTrails)
        {
            if (trail == null)
                continue;

            trail.emitting = emitting;

            if (!emitting)
                trail.Clear();
        }
    }

    public void ResetSlashTracking()
    {
        if (bladePoint != null)
            lastBladePosition = bladePoint.position;

        currentlySlashing = false;
        slashStart = Vector3.zero;
        slashEnd = Vector3.zero;

        SetTrailsEmitting(false);
    }
}