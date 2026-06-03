using UnityEngine;

public class KeepWorldRotation : MonoBehaviour
{
    [Header("World Rotation")]
    [SerializeField] private Vector3 fixedWorldEuler = new Vector3(90f, 0f, 0f);

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(fixedWorldEuler);
    }
}