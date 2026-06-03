using System.Collections;
using UnityEngine;

public class EnemyCutDirection : MonoBehaviour
{
    public enum CutDirection
    {
        Horizontal,
        Vertical,
        DiagonalSlash,
        DiagonalBackslash
    }

    public enum ArrowPointType
    {
        Start,
        End
    }

    [Header("Cut Direction")]
    public CutDirection requiredCutDirection;

    [Header("Arrow")]
    [SerializeField] private float arrowAppearDelay = 1f;

    [Header("Death")]
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private float vanishDuration = 0.8f;
    [SerializeField] private bool shrinkOnDeath = true;

    private Transform arrowTransform;
    private float arrowAngle;

    private bool touchedStartFirst = false;
    private bool isDead = false;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;

        arrowTransform = transform.Find("CutDirectionArrow");

        if (arrowTransform == null)
        {
            Debug.LogWarning("CutDirectionArrow was not found on " + gameObject.name);
            return;
        }

        arrowTransform.gameObject.SetActive(false);

        ////////////////////////////////////requiredCutDirection = (CutDirection)Random.Range(0, 4);
        requiredCutDirection = (CutDirection)Random.Range(0, 2);

        SetArrowAngle();
        UpdateArrowVisual();

        Debug.Log(gameObject.name + " cut direction is " + requiredCutDirection);

        StartCoroutine(ShowArrowAfterDelay());
    }

    private void LateUpdate()
    {
        if (arrowTransform != null && !isDead)
        {
            UpdateArrowVisual();
        }
    }

    private IEnumerator ShowArrowAfterDelay()
    {
        yield return new WaitForSeconds(arrowAppearDelay);

        if (arrowTransform != null && !isDead)
        {
            arrowTransform.gameObject.SetActive(true);
        }
    }

    private void SetArrowAngle()
    {
        switch (requiredCutDirection)
        {
            case CutDirection.Horizontal:
                arrowAngle = 0f;
                break;

            case CutDirection.Vertical:
                arrowAngle = 90f;
                break;

            case CutDirection.DiagonalSlash:
                arrowAngle = 45f;
                break;

            case CutDirection.DiagonalBackslash:
                arrowAngle = -45f;
                break;
        }
    }

    private void UpdateArrowVisual()
    {
        arrowTransform.rotation = Quaternion.Euler(0f, arrowAngle, 0f);
    }

    public void RegisterArrowPointHit(ArrowPointType pointType)
    {
        if (isDead) return;

        if (pointType == ArrowPointType.Start)
        {
            touchedStartFirst = true;
            Debug.Log("Hit START of arrow");
            return;
        }

        if (pointType == ArrowPointType.End)
        {
            if (touchedStartFirst)
            {
                Debug.Log("CORRECT CUT DIRECTION!");
                StartCoroutine(DieAndVanish());
            }
            else
            {
                Debug.Log("WRONG CUT DIRECTION. You hit END first.");
            }

            touchedStartFirst = false;
        }
    }

    private IEnumerator DieAndVanish()
    {
        if (isDead) yield break;
        isDead = true;

        if (arrowTransform != null)
            arrowTransform.gameObject.SetActive(false);

        NinjaMover mover = GetComponent<NinjaMover>();
        if (mover != null)
            mover.enabled = false;

        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && dieSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(dieSound);
        }

        float timer = 0f;

        while (timer < vanishDuration)
        {
            timer += Time.deltaTime;
            float t = timer / vanishDuration;

            if (shrinkOnDeath)
            {
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}