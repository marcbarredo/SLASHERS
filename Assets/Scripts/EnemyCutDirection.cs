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

    public CutDirection requiredCutDirection;

    [SerializeField] private float arrowAppearDelay = 1f;

    private Transform arrowTransform;
    private float arrowAngle;

    private bool touchedStartFirst = false;

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

    public void RegisterArrowPointHit(ArrowPointType pointType)
    {
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

                // Put your enemy death / cut logic here
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("WRONG CUT DIRECTION. You hit END first.");
            }

            touchedStartFirst = false;
        }
    }
}