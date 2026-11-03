using UnityEngine;

public class BarSeat : MonoBehaviour
{
    public Transform anchor;

    public Customer Occupant { get; set; }

    public bool IsFree
    {
        get { return Occupant == null; }
    }

    public Vector3 Position
    {
        get { return anchor != null ? anchor.position : transform.position; }
    }

    public Quaternion Rotation
    {
        get { return anchor != null ? anchor.rotation : transform.rotation; }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsFree ? Color.green : Color.red;
        Gizmos.DrawWireSphere(Position, 0.25f);
        Gizmos.DrawRay(Position + Vector3.up, Rotation * Vector3.forward * 0.6f);
    }
}
