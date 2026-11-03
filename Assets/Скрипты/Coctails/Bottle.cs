using UnityEngine;

public class Bottle : Usable
{
    public IngredientData ingredient;
    public float pourRate = 30f;
    public Vector3 pourTiltEuler = new Vector3(70f, 0f, 0f);

    public float tiltSpeed = 10f;

    private Quaternion _targetRotation;
    private bool _hasTarget;

    private void Start()
    {
        _targetRotation = Quaternion.Euler(heldLocalEuler);
    }

    private void Update()
    {
        if (!_hasTarget || transform.parent == null) return;

        transform.localRotation = tiltSpeed > 0f
            ? Quaternion.Slerp(transform.localRotation, _targetRotation, Time.deltaTime * tiltSpeed)
            : _targetRotation;
    }

    public override void OnPrimaryHeld(Container target, float dt)
    {
        _targetRotation = Quaternion.Euler(heldLocalEuler + pourTiltEuler);
        _hasTarget = true;

        if (target != null) target.Add(ingredient, pourRate * dt);
    }

    public override void OnPrimaryReleased()
    {
        _targetRotation = Quaternion.Euler(heldLocalEuler);
        _hasTarget = true;
    }
}
