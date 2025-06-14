using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleDrive : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 100f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        Vector3 targetVelocity = transform.forward * moveInput * moveSpeed;
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, 0.1f); // Smooth acceleration

        if (moveInput != 0)
        {
            float turn = turnInput * turnSpeed * Time.fixedDeltaTime * Mathf.Sign(moveInput);
            Quaternion rotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * rotation);
        }
    }

}
