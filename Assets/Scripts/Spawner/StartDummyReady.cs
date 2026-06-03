using System.Collections;
using UnityEngine;

public class StartDummyReady : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private int playerId = 1;
    [SerializeField] private string requiredSwordTag = "Blade";
    [SerializeField] private GameFlowManager gameFlowManager;

    [Header("Only This Player Can Cut This Dummy")]
    [SerializeField] private Transform requiredBladeRoot;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string cutTriggerName = "Cut";
    [SerializeField] private string resetStateName = "stand";
    [SerializeField] private float readyDelay = 0.6f;

    private bool alreadyCut;
    private Coroutine cutCoroutine;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void Setup(int newPlayerId, GameFlowManager newGameFlowManager, Transform newBladeRoot)
    {
        playerId = newPlayerId;
        gameFlowManager = newGameFlowManager;
        requiredBladeRoot = newBladeRoot;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyCut)
            return;

        if (!other.CompareTag(requiredSwordTag))
            return;

        if (!IsCorrectPlayerBlade(other.transform))
            return;

        alreadyCut = true;

        if (cutCoroutine != null)
            StopCoroutine(cutCoroutine);

        cutCoroutine = StartCoroutine(CutSequence());
    }

    private bool IsCorrectPlayerBlade(Transform bladeTransform)
    {
        if (requiredBladeRoot == null)
        {
            Debug.LogWarning(gameObject.name + " has no Required Blade Root assigned.");
            return true;
        }

        if (bladeTransform == requiredBladeRoot)
            return true;

        return bladeTransform.IsChildOf(requiredBladeRoot);
    }

    private IEnumerator CutSequence()
    {
        Debug.Log("Dummy cut correctly by Player " + playerId);

        if (animator != null)
        {
            animator.enabled = true;
            animator.ResetTrigger(cutTriggerName);
            animator.SetTrigger(cutTriggerName);
        }

        yield return new WaitForSeconds(readyDelay);

        if (gameFlowManager != null)
            gameFlowManager.RegisterPlayerReady(playerId);
        else
            Debug.LogError("GameFlowManager is not assigned on " + gameObject.name);

        cutCoroutine = null;
    }

    public void ResetDummy()
    {
        alreadyCut = false;

        if (cutCoroutine != null)
        {
            StopCoroutine(cutCoroutine);
            cutCoroutine = null;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.enabled = true;
            animator.ResetTrigger(cutTriggerName);
            animator.Rebind();
            animator.Update(0f);

            animator.Play(resetStateName, 0, 0f);
            animator.Update(0f);
        }
    }
}