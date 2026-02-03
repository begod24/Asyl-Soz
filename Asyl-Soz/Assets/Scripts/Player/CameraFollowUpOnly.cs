using UnityEngine;

public class CameraFollowUpOnly : MonoBehaviour
{
    [Header("Target")]
    [UnityEngine.SerializeField] private Transform target;

    [Header("Follow")]
    [UnityEngine.SerializeField] private float yOffset = 1.0f;
    [UnityEngine.SerializeField] private float smoothSpeed = 10f;

    private float lockedY;

    private void Awake()
    {
        lockedY = transform.position.y;

        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) target = go.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float desiredY = target.position.y + yOffset;
        if (desiredY > lockedY) lockedY = desiredY;

        Vector3 current = transform.position;
        Vector3 desired = new Vector3(current.x, lockedY, current.z);

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(current, desired, t);
    }
}