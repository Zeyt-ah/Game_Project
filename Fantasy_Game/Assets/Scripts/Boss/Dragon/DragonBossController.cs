using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DragonBossController : MonoBehaviour
{
    private enum DragonState
    {
        Circling,
        IntroSequence,
        GroundCombat,
        Attacking,
        Dead
    }

    private enum DragonPhase
    {
        PhaseOne,
        PhaseTwo,
        PhaseThree
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    
    [Header("Boss UI")]
    [SerializeField] private GameObject bossHealthBarPanel;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private string bossDisplayName = "Ashfang";

    [Header("Circling Settings")]
    [SerializeField] private Transform orbitCentre;
    [SerializeField] private float orbitRadiusX = 95f;
    [SerializeField] private float orbitRadiusZ = 110f;
    [SerializeField] private float orbitHeight = 40f;
    [SerializeField] private float orbitSpeed = 0.25f;
    [SerializeField] private float circlingMoveSpeed = 12f;

    [Header("Ground Movement")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float phaseTwoWalkSpeed = 3f;
    [SerializeField] private float phaseThreeWalkSpeed = 4f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float stopDistance = 6f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 300f;
    [SerializeField] private float currentHealth;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float phaseTwoAttackCooldown = 2.2f;
    [SerializeField] private float phaseThreeAttackCooldown = 1.8f;

    [SerializeField] private float regularAttackRange = 7f;
    [SerializeField] private float fireBreathRange = 15f;

    [SerializeField] private float regularAttackDamage = 20f;
    [SerializeField] private float fireBreathDamage = 35f;
    [SerializeField] private float chargeDamage = 45f;

    [Header("Animation State Names")]
    [SerializeField] private string flyingStateName = "FlyingFWD";
    [SerializeField] private string walkStateName = "Walk";
    [SerializeField] private string idleStateName = "IdleSimple";
    [SerializeField] private string battleStanceStateName = "BattleStance";
    [SerializeField] private string regularAttackStateName = "Bite";
    [SerializeField] private string fireBreathStateName = "Drakaris";
    [SerializeField] private string chargeAttackStateName = "FlyingAttack";
    [SerializeField] private string deathStateName = "Die";

    private DragonState currentState = DragonState.Circling;
    private DragonPhase currentPhase = DragonPhase.PhaseOne;

    private float orbitAngle;
    private bool canAttack = true;

    // Sets up the dragon and begins the circling state.
    private void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

        UpdateHealthBar();

        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(false);
        }

        if (bossNameText != null)
        {
            bossNameText.text = bossDisplayName;
        }

        currentState = DragonState.Circling;
        PlayAnimation(flyingStateName);
    }

    // Updates the dragon behaviour depending on its current state
    private void Update()
    {
        if (currentState == DragonState.Dead)
        {
            return;
        }

        if (currentState == DragonState.Circling)
        {
            CircleArena();
            return;
        }

        if (currentState == DragonState.GroundCombat)
        {
            WalkTowardsPlayer();
            TryAttackPlayer();
        }
    }

    // Moves the dragon smoothly above the arena before the boss fight starts.
    private void CircleArena()
    {
        if (orbitCentre == null)
        {
            return;
        }

        orbitAngle += orbitSpeed * Time.deltaTime;

        float x = Mathf.Cos(orbitAngle) * orbitRadiusX;
        float z = Mathf.Sin(orbitAngle) * orbitRadiusZ;

        Vector3 targetPosition = new Vector3(
            orbitCentre.position.x + x,
            orbitCentre.position.y + orbitHeight,
            orbitCentre.position.z + z
        );

        MoveTowardsFlyingTarget(targetPosition, circlingMoveSpeed);
    }

    // Moves the dragon through the air while turning mostly horizontally.
    private void MoveTowardsFlyingTarget(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);

        if (flatDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    // Temporarily pauses the dragon's normal behaviour during the boss intro sequence.
    public void PauseDragonBehaviour()
    {
        currentState = DragonState.IntroSequence;
        canAttack = false;
    }

    // Plays the dragon's battle stance animation.
    public void PlayBattleStance()
    {
        PlayAnimation(battleStanceStateName);
    }

    // Plays the dragon's flying animation
    public void PlayFlyingAnimation()
    {
        PlayAnimation(flyingStateName);
    }

    // Starts ground combat after the intro sequence lands the dragon in the arena.
    public void StartGroundBossFight()
    {
        if (currentState == DragonState.Dead)
        {
            return;
        }

        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(true);
        }

        if (bossNameText != null)
        {
            bossNameText.text = bossDisplayName;
        }

        currentState = DragonState.GroundCombat;
        canAttack = true;

        UpdatePhase();
        UpdateHealthBar();
        PlayAnimation(walkStateName);

        Debug.Log("Dragon starts ground combat.");
    }

    // Slowly moves the dragon toward the player while keeping it on the ground.
    private void WalkTowardsPlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        float distanceToPlayer = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(player.position.x, 0f, player.position.z)
        );

        if (distanceToPlayer > stopDistance)
        {
            transform.position += direction.normalized * walkSpeed * Time.deltaTime;
            PlayAnimation(walkStateName);
        }
        else
        {
            PlayAnimation(idleStateName);
        }
    }

    // Checks whether the dragon should attack the player.
    private void TryAttackPlayer()
    {
        if (player == null || !canAttack)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > fireBreathRange)
        {
            return;
        }

        StartCoroutine(AttackRoutine(distanceToPlayer));
    }

    // Handles attack selection based on the current phase.
    private IEnumerator AttackRoutine(float distanceToPlayer)
    {
        canAttack = false;
        currentState = DragonState.Attacking;

        if (currentPhase == DragonPhase.PhaseOne)
        {
            yield return RegularAttack();
        }
        else if (currentPhase == DragonPhase.PhaseTwo)
        {
            int attackChoice = Random.Range(0, 2);

            if (attackChoice == 0 || distanceToPlayer <= regularAttackRange)
            {
                yield return RegularAttack();
            }
            else
            {
                yield return FireBreathAttack();
            }
        }
        else if (currentPhase == DragonPhase.PhaseThree)
        {
            int attackChoice = Random.Range(0, 3);

            if (attackChoice == 0)
            {
                yield return RegularAttack();
            }
            else if (attackChoice == 1)
            {
                yield return FireBreathAttack();
            }
            else
            {
                yield return ChargeAttack();
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        if (currentState != DragonState.Dead)
        {
            currentState = DragonState.GroundCombat;
            canAttack = true;
            PlayAnimation(walkStateName);
        }
    }

    // Performs the dragon's close-range bite attack.
    private IEnumerator RegularAttack()
    {
        PlayAnimation(regularAttackStateName);

        yield return new WaitForSeconds(0.6f);

        DamagePlayerIfInRange(regularAttackRange, regularAttackDamage);

        yield return new WaitForSeconds(0.8f);
    }

    // Performs the dragon's fire breath attack.
    private IEnumerator FireBreathAttack()
    {
        PlayAnimation(fireBreathStateName);

        yield return new WaitForSeconds(0.8f);

        DamagePlayerIfInRange(fireBreathRange, fireBreathDamage);

        Debug.Log("Dragon uses Fire Breath.");

        yield return new WaitForSeconds(1.4f);
    }

    // Performs the dragon's phase-three charge attack.
    private IEnumerator ChargeAttack()
    {
        PlayAnimation(chargeAttackStateName);

        if (player == null)
        {
            yield break;
        }

        Vector3 chargeDirection = player.position - transform.position;
        chargeDirection.y = 0f;
        chargeDirection.Normalize();

        float chargeTime = 1.2f;
        float elapsedTime = 0f;
        float chargeSpeed = walkSpeed * 4f;

        while (elapsedTime < chargeTime)
        {
            elapsedTime += Time.deltaTime;

            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

            DamagePlayerIfInRange(regularAttackRange, chargeDamage);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    // Damages the player if they are within the selected attack range
    private void DamagePlayerIfInRange(float range, float damage)
    {
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > range)
        {
            return;
        }

        PlayerScriptNew playerScript = player.GetComponent<PlayerScriptNew>();

        if (playerScript != null)
        {
            playerScript.TakeDamage(Mathf.RoundToInt(damage));
            Debug.Log($"Dragon deals {damage} damage to player.");
        }
        else
        {
            Debug.LogWarning("PlayerScriptNew was not found on the player.");
        }
    }

    // Damages the dragon and updates its phase/health bar.
    public void TakeDamage(float damage)
    {
        if (currentState == DragonState.Dead)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdatePhase();
        UpdateHealthBar();

        Debug.Log($"Dragon takes {damage} damage. Current health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Updates the dragon's phase based on its remaining health.
    private void UpdatePhase()
    {
        float healthPercentage = currentHealth / maxHealth;

        if (healthPercentage <= 0.33f)
        {
            currentPhase = DragonPhase.PhaseThree;
            walkSpeed = phaseThreeWalkSpeed;
            attackCooldown = phaseThreeAttackCooldown;
        }
        else if (healthPercentage <= 0.66f)
        {
            currentPhase = DragonPhase.PhaseTwo;
            walkSpeed = phaseTwoWalkSpeed;
            attackCooldown = phaseTwoAttackCooldown;
        }
        else
        {
            currentPhase = DragonPhase.PhaseOne;
        }
    }

    // Updates the dragon health bar
    private void UpdateHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    // Handles the dragon being defeated
    private void Die()
    {
        currentState = DragonState.Dead;
        canAttack = false;

        PlayAnimation(deathStateName);

        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(false);
        }

        Debug.Log("Dragon defeated.");
    }

    // Plays an Animator state by string name.
    private void PlayAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return;
        }

        animator.Play(stateName);
    }
}