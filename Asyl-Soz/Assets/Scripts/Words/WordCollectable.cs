using UnityEngine;
using TMPro;

/// <summary>
/// Attached to a word pickup floating above a platform.
/// Displays a random Kazakh word from the bank and fires a collection event
/// when the player touches it. The spawner creates these automatically,
/// so the designer never has to place individual words by hand.
///
/// WHY this approach:
/// • Words auto-generate at runtime → truly endless, no manual level design.
/// • The event-driven pattern (OnWordCollected) lets any UI subscribe without coupling.
/// • Prefab only needs a TextMeshPro label + Collider2D — no complex setup.
/// </summary>
public class WordCollectable : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    private KazakhWord word;
    private Vector3 startPos;

    /// <summary>Called by the spawner right after Instantiate to assign a word.</summary>
    public void Initialize(KazakhWord wordData)
    {
        word = wordData;
        if (label != null && word != null)
            label.text = word.kazakh;
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Gentle floating bob animation
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = startPos + Vector3.up * offset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var collector = other.GetComponent<WordCollector>();
        if (collector != null && word != null)
            collector.Collect(word);

        Destroy(gameObject);
    }
}
