using UnityEngine;

public class Usable : MonoBehaviour
{
    [Header("Положение в руке")]
    public Vector3 heldLocalPosition = Vector3.zero;
    public Vector3 heldLocalEuler = Vector3.zero;

    public bool autoCenterOnHold = false;

    public virtual void OnPrimaryHeld(Container target, float dt) { }
    public virtual void OnPrimaryReleased() { }
    public virtual void OnSecondaryHeld(Container target, float dt) { }
}
