using System.Collections;
using UnityEngine;

public class StartDummyReady : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private int playerId = 1;
    [SerializeField] private string requiredSwordTag = "Blade";
    [SerializeField] private GameFlowManager gameFlowManager;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string cutTriggerName = "Cut";
    [SerializeField] private string resetStateName = "stand";
    [SerializeField] private float readyDelay = 0.6f;

    private bool alreadyCut = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    private Rigidbody rb;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyCut) return;

        if (!other.CompareTag(requiredSwordTag))
            return;

        alreadyCut = true;
        StartCoroutine(CutSequence());
    }

    private IEnumerator CutSequence()
    {
        Debug.Log("Dummy cut correctly by Player " + playerId);

        if (animator != null)
        {
            animator.ResetTrigger(cutTriggerName);
            animator.SetTrigger(cutTriggerName);
        }

        yield return new WaitForSeconds(readyDelay);

        if (gameFlowManager != null)
        {
            gameFlowManager.RegisterPlayerReady(playerId);
        }
        else
        {
            Debug.LogError("GameFlowManager is not assigned on " + gameObject.name);
        }
    }

    public void ResetDummy()
    {
        alreadyCut = false;

        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.enabled = true;
            animator.applyRootMotion = false;

            animator.ResetTrigger(cutTriggerName);

            animator.Rebind();
            animator.Update(0f);

            animator.Play(resetStateName, 0, 0f);
            animator.Update(0f);
        }

        Debug.Log(gameObject.name + " reset to " + resetStateName);
    }
}