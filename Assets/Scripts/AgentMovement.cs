using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    // ===== Movement =====
    public float calmSpeed = 1.5f;
    public float panicSpeed = 4f;

    private Vector2 direction;

    // ===== Panic =====
    public float panicRadius = 1.2f;
    public float panicChancePerSecond = 0.15f;

    private bool isPanicked = false;
    private float panicExposureTime = 0f;
    public void ForcePanic()
{
    isPanicked = true;
    UpdateColor();
}


    // ===== Crowding =====
    public float crowdRadius = 0.8f;
    public int crowdThreshold = 6;

    // ===== Stampede =====
public float stampedePressureTime = 1.8f;
private float timeUnderPressure = 0f;

    // ===== Visual =====
    private SpriteRenderer sprite;

    void Start()
    {
        direction = Random.insideUnitCircle.normalized;

        sprite = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    void Update()
    {
        float speed = isPanicked ? panicSpeed : calmSpeed;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Bounce off screen edges
        if (Mathf.Abs(transform.position.x) > 8f)
            direction.x *= -1;

        if (Mathf.Abs(transform.position.y) > 4.5f)
            direction.y *= -1;

        TrySpreadPanic();

        CheckStampedeDeath();
    }

    // ===== Panic Logic =====
    void TrySpreadPanic()
    {
        if (isPanicked)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            panicRadius
        );

        bool nearPanic = false;

        foreach (var hit in hits)
        {
            AgentMovement other = hit.GetComponent<AgentMovement>();
            if (other != null && other.isPanicked)
            {
                nearPanic = true;
                panicExposureTime += Time.deltaTime;

                float chance = panicChancePerSecond;

                if (IsOvercrowded())
                    chance *= 1.5f;

                if (panicExposureTime > 1.5f &&
                    Random.value < chance * Time.deltaTime)
                {
                    isPanicked = true;
                    UpdateColor();
                }

                break;
            }
        }

        if (!nearPanic)
        {
            panicExposureTime = 0f;
        }
    }

    // ===== Crowding =====
    bool IsOvercrowded()
    {
        int count = 0;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            crowdRadius
        );

        foreach (var hit in hits)
        {
            if (hit.GetComponent<AgentMovement>() != null)
                count++;
        }

        return count >= crowdThreshold;
    }

    void CheckStampedeDeath()
{
    if (!isPanicked)
    {
        timeUnderPressure = 0f;
        return;
    }

    if (!IsOvercrowded())
    {
        timeUnderPressure = 0f;
        return;
    }

    timeUnderPressure += Time.deltaTime;

    if (timeUnderPressure >= stampedePressureTime)
    {
        Die();
    }
}

void Die()
{
    sprite.color = Color.black;
Destroy(gameObject, 0.3f);
}

    // ===== Visual =====
    void UpdateColor()
{
    if (sprite == null)
        return;

    sprite.color = isPanicked ? Color.red : Color.white;
}

}
