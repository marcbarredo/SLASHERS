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

    public CutDirection requiredCutDirection;

    [SerializeField] private float arrowAppearDelay = 1f;

    private Transform arrowTransform;
    private float arrowAngle;

    private void Start()
    {
        arrowTransform = transform.Find("CutDirectionArrow");

        if (arrowTransform == null)
        {
            Debug.LogWarning("CutDirectionArrow was not found on " + gameObject.name);
            return;
        }

        arrowTransform.gameObject.SetActive(false);

        requiredCutDirection = (CutDirection)Random.Range(0, 4);

        SetArrowAngle();
        UpdateArrowVisual();

        Debug.Log(gameObject.name + " cut direction is " + requiredCutDirection);

        StartCoroutine(ShowArrowAfterDelay());
    }

    private void LateUpdate()
    {
        if (arrowTransform != null)
        {
            UpdateArrowVisual();
        }
    }

    private IEnumerator ShowArrowAfterDelay()
    {
        yield return new WaitForSeconds(arrowAppearDelay);

        if (arrowTransform != null)
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
        // WORLD SPACE rotation
        arrowTransform.rotation = Quaternion.Euler(0f, arrowAngle, 0f);
    }
}