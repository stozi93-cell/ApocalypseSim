using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    // ===== Movement =====
    public float calmSpeed = 1.5f;
    public float panicSpeed = 4f;

    private Vector2 direction;

    // ===== Panic =====
    public float panicRadius = 2f;
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
    public int crowdThreshold = 3;

    // ===== Stampede =====
public float stampedePressureTime = 1.2f;
private float timeUnderPressure = 0f;

// ===== Stampede State =====
public float stampedeDetectTime = 0.4f;
private float stampedeTimer = 0f;
private bool inStampede = false;

// ===== Stampede Movement =====
public float herdAlignStrength = 25f;
public float dangerRepelStrength = 20f;
private float mobilityFactor = 1f;


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
        float baseSpeed = isPanicked ? panicSpeed : calmSpeed;
float speed = baseSpeed * mobilityFactor;

        Vector2 moveDir = direction;

if (isPanicked && IsOvercrowded())
{
    Vector2 herd = GetHerdDirection() * herdAlignStrength;
    Vector2 repel = GetDangerRepulsion() * dangerRepelStrength;

    Vector2 targetDir = (herd + repel).normalized;

// EXTREME cohesion: almost no individuality
direction = Vector2.Lerp(
    direction,
    targetDir,
    Time.deltaTime * 12f
).normalized;

moveDir = direction;


}

if (!IsInsideDanger())
{
    transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
}
else
{
    // Push agent sideways when blocked (creates compression)
    Vector2 sideways = new Vector2(-moveDir.y, moveDir.x);
    transform.position += (Vector3)(sideways * speed * 0.3f * Time.deltaTime);
}

        // Bounce off screen edges
        if (Mathf.Abs(transform.position.x) > 8f)
            direction.x *= -1;

        if (Mathf.Abs(transform.position.y) > 4.5f)
            direction.y *= -1;

        TrySpreadPanic();

        CheckStampedeDeath();

        DetectStampede();
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

                if (panicExposureTime > 0.8f &&
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

    bool IsInsideDanger()
{
    DangerSource[] dangers = Object.FindObjectsByType<DangerSource>(
        FindObjectsSortMode.None
    );

    foreach (var danger in dangers)
    {
        float dist = Vector2.Distance(transform.position, danger.transform.position);
        if (dist < danger.radius * 0.6f)
            return true;
    }

    return false;
}

    void CheckStampedeDeath()
{
    if (!isPanicked)
    {
        timeUnderPressure = 0f;
        mobilityFactor = 1f;
        return;
    }

    if (IsOvercrowded())
    {
        timeUnderPressure += Time.deltaTime;
        mobilityFactor -= Time.deltaTime * 0.6f;
    }
    else
    {
        timeUnderPressure -= Time.deltaTime;
        mobilityFactor += Time.deltaTime * 0.8f;
    }

    timeUnderPressure = Mathf.Clamp(timeUnderPressure, 0f, 3f);
    mobilityFactor = Mathf.Clamp(mobilityFactor, 0.1f, 1f);

    if (timeUnderPressure >= stampedePressureTime)
    {
        Die();
    }
}

void DetectStampede()
{
    if (isPanicked && IsOvercrowded())
    {
        stampedeTimer += Time.deltaTime;

        if (!inStampede && stampedeTimer >= stampedeDetectTime)
        {
            inStampede = true;
            StampedeManager.ReportStampedeStart(transform.position);
        }
    }
    else
    {
        stampedeTimer = 0f;
        inStampede = false;
    }
    Debug.Log("STAMPede ENTERED at " + transform.position);
}

void Die()
{
    if (inStampede)
    {
        StampedeManager.ReportStampedeDeath(transform.position);
    }

    Destroy(gameObject);
}

    // ===== Visual =====
    void UpdateColor()
{
    if (sprite == null)
        return;

    sprite.color = isPanicked ? Color.red : Color.white;
}

Vector2 GetHerdDirection()
{
    Vector2 avgDirection = Vector2.zero;
    int count = 0;

    Collider2D[] hits = Physics2D.OverlapCircleAll(
        transform.position,
        crowdRadius
    );

    foreach (var hit in hits)
    {
        AgentMovement other = hit.GetComponent<AgentMovement>();
        if (other != null && other != this)
        {
            avgDirection += other.direction;
            count++;
        }
    }

    if (count == 0)
        return direction;

    return avgDirection.normalized;
}

Vector2 GetDangerRepulsion()
{
    Vector2 repel = Vector2.zero;

    // Repel from panicked agents
    Collider2D[] hits = Physics2D.OverlapCircleAll(
        transform.position,
        panicRadius
    );

    foreach (var hit in hits)
    {
        AgentMovement other = hit.GetComponent<AgentMovement>();
        if (other != null && other.isPanicked)
        {
            Vector2 away = (Vector2)(transform.position - other.transform.position);
            repel += away.normalized;
        }
    }

    // Repel from danger sources
    DangerSource[] dangers = Object.FindObjectsByType<DangerSource>(
    FindObjectsSortMode.None
);
    foreach (var danger in dangers)
    {
        float dist = Vector2.Distance(transform.position, danger.transform.position);
        if (dist < danger.radius)
        {
            Vector2 away = (Vector2)(transform.position - danger.transform.position);
            float intensity = 1f - (dist / danger.radius);
            repel += away.normalized * intensity * danger.strength;
        }
    }

    return repel.normalized;
}

}
