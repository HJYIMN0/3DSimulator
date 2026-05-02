using UnityEngine;
using UnityEngine.Events;

public class GroundChecker : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Coyote / Buffering")]
    [SerializeField] private float coyoteTimeDuration = 0.2f;
    [SerializeField] private int jumpBufferFrames = 5;

    public UnityEvent<bool> onGroundStateChanged;

    private bool isGrounded;
    public bool IsGrounded => isGrounded;

    private bool isInCoyoteTime;
    public bool IsInCoyoteTime => isInCoyoteTime;
    public bool IsGroundedBuffered => isGrounded || isInCoyoteTime;

    private float coyoteStartTime;
    private int jumpFramesRemaining;

    void Awake()
    {
        if (groundCheckPoint == null)
        {
            groundCheckPoint = transform;
            Debug.LogWarning("GroundCheckPoint non assegnato, uso il Transform del Player");
        }
    }

    public void TriggerJump()
    {
        jumpFramesRemaining = jumpBufferFrames;
    }

    void Update()
    {
        if (jumpFramesRemaining > 0)
        {
            jumpFramesRemaining--;
            return;
        }

        Vector3 checkPos = groundCheckPoint.position;
        Collider[] hits = Physics.OverlapSphere(checkPos, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        bool sphereHit = hits.Length > 0;

        if (sphereHit)
        {
            if (!isGrounded)
            {
                isGrounded = true;
                onGroundStateChanged?.Invoke(true);
            }
            isInCoyoteTime = false;
            coyoteStartTime = 0f;
        }
        else
        {
            if (isGrounded)
            {
                isGrounded = false;
                onGroundStateChanged?.Invoke(false);
                isInCoyoteTime = true;
                coyoteStartTime = Time.time;
            }
            else if (isInCoyoteTime && (Time.time - coyoteStartTime > coyoteTimeDuration))
            {
                isInCoyoteTime = false;
                coyoteStartTime = 0f;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
    

}
