using UnityEngine;

public class DebugPlayerDragMovement : MonoBehaviour
{
    [Header("Test Movement")]
    [SerializeField] private float moveSpeed = 8f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            input.z += 1f;

        if (Input.GetKey(KeyCode.S))
            input.z -= 1f;

        if (Input.GetKey(KeyCode.D))
            input.x += 1f;

        if (Input.GetKey(KeyCode.A))
            input.x -= 1f;

        input = input.normalized;

        if (rb == null)
        {
            transform.position += input * moveSpeed * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            input.z += 1f;

        if (Input.GetKey(KeyCode.S))
            input.z -= 1f;

        if (Input.GetKey(KeyCode.D))
            input.x += 1f;

        if (Input.GetKey(KeyCode.A))
            input.x -= 1f;

        input = input.normalized;

        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}