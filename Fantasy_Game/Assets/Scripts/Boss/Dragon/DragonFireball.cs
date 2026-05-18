using UnityEngine;

public class DragonFireball : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;

    private Vector3 targetPosition;
    private float speed;
    private float lifetime;
    private float impactDamage;
    private GameObject lingeringFirePrefab;
    private float lingeringFireDuration;

    private bool hasBeenSetup;
    private bool hasExploded;
    private Collider fireballCollider;

    // Sets up the fireball's movement and damage values
    public void Setup(
        Vector3 newTargetPosition,
        float newSpeed,
        float newLifetime,
        float newImpactDamage,
        GameObject newLingeringFirePrefab,
        float newLingeringFireDuration)
    {
        targetPosition = newTargetPosition;
        speed = newSpeed;
        lifetime = newLifetime;
        impactDamage = newImpactDamage;
        lingeringFirePrefab = newLingeringFirePrefab;
        lingeringFireDuration = newLingeringFireDuration;

        hasBeenSetup = true;
        hasExploded = false;

        fireballCollider = GetComponent<Collider>();

        Destroy(gameObject, lifetime);
    }

    // Moves the fireball towards its target
    private void Update()
    {
        if (!hasBeenSetup || hasExploded)
        {
            return;
        }

        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude <= 0.25f)
        {
            Explode();
            return;
        }

        transform.position += direction.normalized * speed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }

    // Handles the fireball hitting something
    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenSetup || hasExploded)
        {
            return;
        }

        if (other.GetComponentInParent<DragonBossController>() != null)
        {
            return;
        }

        DragonLingeringFireArea existingFire = other.GetComponentInParent<DragonLingeringFireArea>();

        if (existingFire != null)
        {
            return;
        }

        PlayerScriptNew playerScript = other.GetComponent<PlayerScriptNew>();

        if (playerScript == null)
        {
            playerScript = other.GetComponentInParent<PlayerScriptNew>();
        }

        if (playerScript != null)
        {
            playerScript.TakeDamage(Mathf.RoundToInt(impactDamage));
        }

        Explode();
    }

    // Creates lingering fire flat on the ground and destroys the fireball
    private void Explode()
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;

        if (fireballCollider != null)
        {
            fireballCollider.enabled = false;
        }

        Vector3 fireSpawnPosition = GetGroundPosition();

        if (lingeringFirePrefab != null)
        {
            GameObject lingeringFireObject = Instantiate(
                lingeringFirePrefab,
                fireSpawnPosition,
                Quaternion.identity
            );

            DragonLingeringFireArea lingeringFire = lingeringFireObject.GetComponent<DragonLingeringFireArea>();

            if (lingeringFire != null)
            {
                lingeringFire.Setup(lingeringFireDuration);
            }
        }

        Destroy(gameObject);
    }

    // Finds the ground position below the fireball target
    private Vector3 GetGroundPosition()
    {
        Vector3 rayStartPosition = targetPosition + Vector3.up * 20f;

        RaycastHit hit;

        if (groundLayer.value != 0)
        {
            if (Physics.Raycast(rayStartPosition, Vector3.down, out hit, 50f, groundLayer))
            {
                return hit.point + Vector3.up * 0.03f;
            }
        }
        else
        {
            if (Physics.Raycast(rayStartPosition, Vector3.down, out hit, 50f))
            {
                return hit.point + Vector3.up * 0.03f;
            }
        }

        return new Vector3(targetPosition.x, 0.03f, targetPosition.z);
    }
}