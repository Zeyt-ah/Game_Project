using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DragonBossController : MonoBehaviour
{
    private enum DragonState
    {
        Waiting,
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
    [SerializeField] private GameManagerScript gameManager;

    [Header("Egg Requirement")]
    [SerializeField] private int eggsRequiredToStartCircling = 5;
    [SerializeField] private bool startCirclingOnlyAfterEggs = true;

    [Header("Boss UI")]
    [SerializeField] private GameObject bossHealthBarPanel;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private string bossDisplayName = "Ashfang";

    [Header("Audio")]
    [SerializeField] private AudioSource dragonAudioSource;
    [SerializeField] private AudioClip roarClip;
    [SerializeField] private AudioClip biteClip;
    [SerializeField] private AudioClip fireballClip;
    [SerializeField] private float roarVolume = 1f;
    [SerializeField] private float biteVolume = 0.8f;
    [SerializeField] private float fireballVolume = 0.8f;

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

    [Tooltip("Distance from the dragon body where the dragon stops walking")]
    [SerializeField] private float stopDistance = 12f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 300f;
    [SerializeField] private float currentHealth;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float phaseTwoAttackCooldown = 2.2f;
    [SerializeField] private float phaseThreeAttackCooldown = 1.8f;

    [SerializeField] private float regularAttackRange = 7f;

    [Header("Attack Damage")]
    [SerializeField] private float regularAttackDamage = 20f;
    [SerializeField] private float phaseTwoRegularAttackDamage = 25f;
    [SerializeField] private float phaseThreeRegularAttackDamage = 35f;

    [Header("Attack Points")]
    [SerializeField] private Transform biteAttackPoint;

    [Header("Fireball Attack")]
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject lingeringFirePrefab;
    [SerializeField] private float fireballSpeed = 18f;
    [SerializeField] private float fireballLifetime = 4f;
    [SerializeField] private float fireballImpactDamage = 15f;
    [SerializeField] private float lingeringFireDuration = 5f;
    [SerializeField] private float fireballAttackRange = 35f;
    [SerializeField] private float fireballAttackAngle = 45f;

    [Header("Animation State Names")]
    [SerializeField] private string flyingStateName = "FlyingFWD";
    [SerializeField] private string walkStateName = "Walk";
    [SerializeField] private string idleStateName = "IdleSimple";
    [SerializeField] private string battleStanceStateName = "BattleStance";
    [SerializeField] private string regularAttackStateName = "Bite";
    [SerializeField] private string fireBallStateName = "Drakaris";
    [SerializeField] private string deathStateName = "Die";

    private DragonState currentState = DragonState.Circling;
    private DragonPhase currentPhase = DragonPhase.PhaseOne;
    private float currentRegularAttackDamage;

    private float orbitAngle;
    private bool canAttack = true;
    private float groundCombatY;
    private bool lockGroundCombatHeight;
    private float nextAttackAllowedTime;

    // Sets up the dragon and begins the circling state 
    private void Start()
    {
        currentHealth = maxHealth;
        currentRegularAttackDamage = regularAttackDamage;

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

        if (bossNameText != null)
        {
            bossNameText.text = bossDisplayName;
        }

        UpdateHealthBar();

        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(false);
        }

        if (startCirclingOnlyAfterEggs)
        {
            currentState = DragonState.Waiting;
            PlayAnimation(idleStateName);
        }
        else
        {
            currentState = DragonState.Circling;
            PlayAnimation(flyingStateName);
        }
    }

    // Updates the dragon behaviour depending on its current state 
    private void Update()
    {
        if (currentState == DragonState.Dead)
        {
            return;
        }

        if (currentState == DragonState.Waiting)
        {
            CheckEggRequirement();
            return;
        }

        if (currentState == DragonState.Circling)
        {
            CircleArena();
            return;
        }

        if (currentState == DragonState.GroundCombat)
        {
            HandleGroundCombat();
        }
    }

    // Starts the dragon circling once the player has collected enough eggs
    private void CheckEggRequirement()
    {
        if (gameManager == null)
        {
            return;
        }

        if (gameManager.EggCount() < eggsRequiredToStartCircling)
        {
            return;
        }

            currentState = DragonState.Circling;
            PlayAnimation(flyingStateName);

            Debug.Log("Dragon starts circling because " + gameManager.EggCount() + " eggs have been collected.");
    }

    // Handles walking, facing, and attacking during the ground fight
    private void HandleGroundCombat()
    {
        if (player == null)
        {
            return;
        }

        FacePlayerHorizontally();
        LockDragonToGroundHeight();

        if (!canAttack)
        {
            return;
        }

        float bodyDistanceToPlayer = GetHorizontalDistance(transform.position, player.position);
        float biteDistanceToPlayer = GetHorizontalDistanceFromAttackPointToPlayer(biteAttackPoint);

        Debug.Log("Body distance: " + bodyDistanceToPlayer + " | Bite distance: " + biteDistanceToPlayer);

        if (Time.time < nextAttackAllowedTime)
        {
            WalkTowardsPlayer();
            return;
        }

        bool canBite = ShouldStartBiteAttack(biteDistanceToPlayer);
        bool canUseFireball = ShouldUseLongRangeAttack();

        if (canBite || canUseFireball)
        {
            StartCoroutine(AttackRoutine(canBite, canUseFireball));
            return;
        }

        WalkTowardsPlayer();
    }

    // Moves the dragon smoothly above the arena before the boss fight starts 
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

    // Moves the dragon through the air while turning mostly horizontally
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

    // Temporarily pauses the dragons normal behaviour during the boss intro sequence
    public void PauseDragonBehaviour()
    {
        currentState = DragonState.IntroSequence;
        canAttack = false;
    }

    // Plays the dragons battle stance animation
    public void PlayBattleStance()
    {
        PlayAnimation(battleStanceStateName);
    }

    // Plays the dragons flying animation
    public void PlayFlyingAnimation()
    {
        PlayAnimation(flyingStateName);
    }

    // Starts ground combat after the intro sequence lands the dragon in the arena
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

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayDragonBossMusic();
        }

        groundCombatY = transform.position.y;
        lockGroundCombatHeight = true;

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        currentState = DragonState.GroundCombat;
        canAttack = true;

        UpdatePhase();
        UpdateHealthBar();
        PlayAnimation(walkStateName);

        Debug.Log("Dragon starts ground combat at Y height: " + groundCombatY);
    }

    // Turns the dragon to face the player without tilting up or down
    private void FacePlayerHorizontally()
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
    }

    // Moves the dragon towards the player until the body reaches the stopping distance
    private void WalkTowardsPlayer()
    {
        if (player == null)
        {
            return;
        }

        float bodyDistanceToPlayer = GetHorizontalDistance(transform.position, player.position);

        if (bodyDistanceToPlayer <= stopDistance)
        {
            PlayAnimation(idleStateName);
            LockDragonToGroundHeight();
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            LockDragonToGroundHeight();
            return;
        }

        Vector3 newPosition = transform.position + direction.normalized * walkSpeed * Time.deltaTime;

        if (lockGroundCombatHeight)
        {
            newPosition.y = groundCombatY;
        }

        transform.position = newPosition;

        PlayAnimation(walkStateName);
    }

    // Keeps the dragon fixed to its landing height during ground combat
    private void LockDragonToGroundHeight()
    {
        if (!lockGroundCombatHeight)
        {
            return;
        }

        Vector3 lockedPosition = transform.position;
        lockedPosition.y = groundCombatY;
        transform.position = lockedPosition;
    }

    // Checks whether the dragon should begin a bite attack
    private bool ShouldStartBiteAttack(float biteDistanceToPlayer)
    {
        return biteDistanceToPlayer <= regularAttackRange;
    }

    // Checks whether the dragon should use its fireball attack
    private bool ShouldUseLongRangeAttack()
    {
        if (currentPhase == DragonPhase.PhaseOne)
        {
            return false;
        }

        return IsPlayerInFireballCone();
    }

    // Checks whether the player is in front of the dragon and within fireball range
    private bool IsPlayerInFireballCone()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        if (fireballSpawnPoint != null)
        {
            origin = fireballSpawnPoint.position;
            forward = fireballSpawnPoint.forward;
        }

        Vector3 directionToPlayer = player.position - origin;
        directionToPlayer.y = 0f;

        forward.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.01f || forward.sqrMagnitude <= 0.01f)
        {
            return false;
        }

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > fireballAttackRange)
        {
            return false;
        }

        float angleToPlayer = Vector3.Angle(forward.normalized, directionToPlayer.normalized);

        Debug.Log("Fireball distance: " + distanceToPlayer + " | angle: " + angleToPlayer);

        return angleToPlayer <= fireballAttackAngle;
    }

    // Handles attack selection based on which attacks are currently valid
    private IEnumerator AttackRoutine(bool canBite, bool canUseFireball)
    {
        canAttack = false;
        currentState = DragonState.Attacking;

        if (currentPhase == DragonPhase.PhaseOne)
        {
            if (canBite)
            {
                yield return RegularAttack();
            }
        }
        else if (currentPhase == DragonPhase.PhaseTwo || currentPhase == DragonPhase.PhaseThree)
        {
            if (canBite && canUseFireball)
            {
                int attackChoice = Random.Range(0, 2);

                if (attackChoice == 0)
                {
                    yield return RegularAttack();
                }
                else
                {
                    yield return FireBallAttack();
                }
            }
            else if (canBite)
            {
                yield return RegularAttack();
            }
            else if (canUseFireball)
            {
                yield return FireBallAttack();
            }
            else
            {
                WalkTowardsPlayer();
            }
        }

        if (currentState != DragonState.Dead)
        {
            nextAttackAllowedTime = Time.time + attackCooldown;
            currentState = DragonState.GroundCombat;
            canAttack = true;
            PlayAnimation(walkStateName);
        }
    }

    // Performs the dragons close-range bite attack
    private IEnumerator RegularAttack()
    {
        PlayAnimation(regularAttackStateName);

        yield return new WaitForSeconds(0.6f);

        PlayBiteSound();
        DamagePlayerIfInRange(biteAttackPoint, regularAttackRange, currentRegularAttackDamage);

        yield return new WaitForSeconds(0.8f);
    }

    
    // Performs the dragon's fireball attack
    private IEnumerator FireBallAttack()
    {
        PlayAnimation(fireBallStateName);

        yield return new WaitForSeconds(0.35f);

        PlayFireballSound();
        SpawnFireball();

        yield return new WaitForSeconds(0.8f);
    }

    // Damages the player if they are within range of the selected attack point
    private void DamagePlayerIfInRange(Transform attackPoint, float range, float damage)
    {
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = GetHorizontalDistanceFromAttackPointToPlayer(attackPoint);

        Debug.Log("Attack distance from " + GetAttackPointName(attackPoint) + " to player: " + distanceToPlayer);

        if (distanceToPlayer > range)
        {
            return;
        }

        PlayerScriptNew playerScript = player.GetComponent<PlayerScriptNew>();

        if (playerScript == null)
        {
            playerScript = player.GetComponentInParent<PlayerScriptNew>();
        }

        if (playerScript != null)
        {
            playerScript.TakeDamage(Mathf.RoundToInt(damage));
            Debug.Log("Dragon deals " + damage + " damage to player from " + GetAttackPointName(attackPoint) + ".");
        }
        else
        {
            Debug.LogWarning("PlayerScriptNew was not found on the player.");
        }
    }

    // Spawns a fireball from the dragons mouth and sends it towards the player
    private void SpawnFireball()
    {
        if (fireballPrefab == null || fireballSpawnPoint == null || player == null)
        {
            Debug.LogWarning("Cannot spawn fireball because a reference is missing.");
            return;
        }

        GameObject fireballObject = Instantiate(
            fireballPrefab,
            fireballSpawnPoint.position,
            fireballSpawnPoint.rotation
        );

        DragonFireball fireball = fireballObject.GetComponent<DragonFireball>();

        if (fireball == null)
        {
            Debug.LogWarning("DragonFireball script is missing from the fireball prefab.");
            return;
        }

        Vector3 targetPosition = player.position;

        RaycastHit hit;

        if (Physics.Raycast(player.position + Vector3.up * 3f, Vector3.down, out hit, 10f))
        {
            targetPosition = hit.point;
        }

        fireball.Setup(
            targetPosition,
            fireballSpeed,
            fireballLifetime,
            fireballImpactDamage,
            lingeringFirePrefab,
            lingeringFireDuration
        );
    }

    // Gets the horizontal distance from an attack point to the player
    private float GetHorizontalDistanceFromAttackPointToPlayer(Transform attackPoint)
    {
        if (player == null)
        {
            return Mathf.Infinity;
        }

        Vector3 origin = transform.position;

        if (attackPoint != null)
        {
            origin = attackPoint.position;
        }

        return GetHorizontalDistance(origin, player.position);
    }

    // Gets the horizontal distance between two world positions
    private float GetHorizontalDistance(Vector3 firstPosition, Vector3 secondPosition)
    {
        firstPosition.y = 0f;
        secondPosition.y = 0f;

        return Vector3.Distance(firstPosition, secondPosition);
    }

    // Gets a safe name for the selected attack point.
    private string GetAttackPointName(Transform attackPoint)
    {
        if (attackPoint == null)
        {
            return "Dragon Root";
        }

        return attackPoint.name;
    }

    // Damages the dragon and updates its phase/health bar
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

        Debug.Log("Dragon takes " + damage + " damage. Current health: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Updates the dragons phase based on its remaining health
    private void UpdatePhase()
    {
        float healthPercentage = currentHealth / maxHealth;

        if (healthPercentage <= 0.33f)
        {
            currentPhase = DragonPhase.PhaseThree;
            walkSpeed = phaseThreeWalkSpeed;
            attackCooldown = phaseThreeAttackCooldown;
            currentRegularAttackDamage = phaseThreeRegularAttackDamage;
        }
        else if (healthPercentage <= 0.66f)
        {
            currentPhase = DragonPhase.PhaseTwo;
            walkSpeed = phaseTwoWalkSpeed;
            attackCooldown = phaseTwoAttackCooldown;
            currentRegularAttackDamage = phaseTwoRegularAttackDamage;
        }
        else
        {
            currentPhase = DragonPhase.PhaseOne;
            currentRegularAttackDamage = regularAttackDamage;
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

        StopAllCoroutines();

        PlayAnimation(deathStateName);

        Collider[] dragonColliders = GetComponentsInChildren<Collider>();

        foreach (Collider dragonCollider in dragonColliders)
        {
            dragonCollider.enabled = false;
        }

        Rigidbody dragonRigidbody = GetComponent<Rigidbody>();

        if (dragonRigidbody != null)
        {
            dragonRigidbody.linearVelocity = Vector3.zero;
            dragonRigidbody.angularVelocity = Vector3.zero;
            dragonRigidbody.isKinematic = true;
        }

        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(false);
        }

        Debug.Log("Dragon defeated.");
        Win();
    }

    private void Win()
    {
        Debug.Log("you win");
        gameManager.Win();
    }

    // Plays an Animator state by string name
    private void PlayAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return;
        }

        animator.Play(stateName);
    }

    // Draws attack and stopping distances in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (biteAttackPoint != null)
        {
            Gizmos.DrawWireSphere(biteAttackPoint.position, regularAttackRange);
        }
    }

    // Plays the dragon roar sound effect
    public void PlayRoarSound()
    {
        if (dragonAudioSource == null || roarClip == null)
        {
            Debug.LogWarning("Dragon roar AudioSource or AudioClip is missing.");
            return;
        }

        dragonAudioSource.PlayOneShot(roarClip, roarVolume);
    }

    // Plays the dragon bite sound effect without interrupting music
    private void PlayBiteSound()
    {
        if (dragonAudioSource == null || biteClip == null)
        {
            Debug.LogWarning("Dragon bite AudioSource or AudioClip is missing.");
            return;
        }

        dragonAudioSource.PlayOneShot(biteClip, biteVolume);
    }

    // Plays the dragon fireball sound effect without interrupting music
    private void PlayFireballSound()
    {
        if (dragonAudioSource == null || fireballClip == null)
        {
            Debug.LogWarning("Dragon fireball AudioSource or AudioClip is missing.");
            return;
        }

        dragonAudioSource.PlayOneShot(fireballClip, fireballVolume);
    }
}