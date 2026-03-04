using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [UnityEngine.SerializeField] private float moveDistance = 2f;
    [UnityEngine.SerializeField] private float moveSpeed = 1.5f;

    private Vector3 startPos;
    private float timer;

    private void Start()
    {
        startPos = transform.position;
        timer = Random.Range(0f, 10f);
    }

    private void Update()
    {
        timer += Time.deltaTime * moveSpeed;
        float offset = Mathf.Sin(timer) * moveDistance;
        transform.position = startPos + Vector3.right * offset;
    }
}