using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeldItem : MonoBehaviour
{
    private Rigidbody rb;
    private Collider[] colliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void OnPickedUp(Transform holdPoint)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        SetCollidersEnabled(false);

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDropped(Vector3 dropPosition, Quaternion dropRotation)
    {
        transform.SetParent(null);
        transform.position = dropPosition;
        transform.rotation = dropRotation;

        SetCollidersEnabled(true);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (Collider col in colliders)
            col.enabled = enabled;
    }
}