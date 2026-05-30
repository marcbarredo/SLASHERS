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
    [SerializeField] private string resetStateName = "idle";
    [SerializeField] private float readyDelay = 0.6f;

    private bool alreadyCut = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        ResetDummy();
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

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger(cutTriggerName);

            // Fully reset animator internal state
            animator.Rebind();
            animator.Update(0f);

            // Force initial pose/state
            animator.Play(resetStateName, 0, 0f);
            animator.Update(0f);
        }
    }
}