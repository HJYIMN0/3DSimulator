using UnityEngine;
public class GroundDetector : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform pointGroundDetector;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Impostazioni Rilevamento")]
    [SerializeField] private float distance = 0.2f;
    [SerializeField] private float sphereRadius = 0.1f;

    [Header("Debug")]
    public bool IsOnGround;

    private Vector3 startPoint;
    private Vector3 direction;

    private RaycastHit hitGround;

    public bool IsGrounded { get; private set; }
    public float SlopeAngle { get; private set; }
    public Vector3 GroundNormal { get; private set; }

    public bool OnSteepSlope { get; set; }

    public RaycastHit HitGround => hitGround;
    public Transform PointGround => pointGroundDetector;

    private void Update() => CheckIfGrounded();

    private void CheckIfGrounded()
    {
        if (!pointGroundDetector) return;

        startPoint = pointGroundDetector.position;
        direction = -transform.up;

        IsGrounded = Physics.SphereCast(startPoint, sphereRadius, direction, out hitGround, distance, whatIsGround);

        if (IsGrounded)
        {
            GroundNormal = HitGround.normal;
            SlopeAngle = Vector3.Angle(HitGround.normal, Vector3.up);
        }
        else GroundNormal = Vector3.up;

        if (IsGrounded != IsOnGround) IsOnGround = IsGrounded;
    }

    public bool ShootGround(float dist) => Physics.SphereCast(startPoint, sphereRadius, direction, out hitGround, dist, whatIsGround);

    private void OnDrawGizmos()
    {
        if (!pointGroundDetector) return;

        Gizmos.DrawWireSphere(pointGroundDetector.position + -transform.up * distance, sphereRadius);

        if (IsGrounded) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
    }
}