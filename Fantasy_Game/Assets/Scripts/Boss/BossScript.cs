using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossScript : MonoBehaviour
{

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;
    public GameManagerScript gameManager;
    public GameObject shieldVisual;
    public GameObject hand;
    public GameObject spell;
    public GameObject sparks;
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip spellSound;
    public AudioClip aoeSound;
    public AudioClip minionSound;

    [Header("UI")]
    public Image bossHealthBar;


    [Header("Stats")]
    public float activationRange = 10f;
    public float attackRange = 1f;
    public float attackCooldown = 1.5f;
    public int maxHealth = 10;


    private int currentPhase = 1; //used for different phases based on HP remaining
    private int currentHealth;
    private float attackTimer = 0f;
    private bool isDead = false;
    private float IFrames = 0.5f;
    private bool canDamage = true;
    private bool shieldActive = true;
    private int hitsTaken = 0;
    private bool startedFight = false;



    //used for runaway behaviour after exiting tired state.
    private Vector3 runAwayTarget;
    public Transform[] escapePoints;
    private float normalSpeed = 5f;
    private float runAwaySpeed = 10f;
    private bool shieldAnimationHappened = false;

    //for minions attack
    public GameObject minion;
    public Transform[] minionsSpawnpoints;
    public int currentMinionCount = 0;
    private int maxMinionCount = 3;

    //for AOE attack
    public GameObject spellAOEIndicator;
    public GameObject spellAOE;

    private bool isTiredCoroutineRunning = false;
    private bool isAttacking = false;

    private enum State {Start,Idle, Chasing, Attacking, Tired, RunAway, Dead }
    private State currentState = State.Start;

    //for attack types
    private enum AttackType { Spell, Minions, AOE}


    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        UpdatePhase();
        attackTimer -= Time.deltaTime;
        if (shieldVisual) shieldVisual.SetActive(shieldActive);

        switch (currentState)
        {
            case State.Start:
                StartBehaviour();
                break;
            case State.Idle:
                IdleBehaviour();
                break;
            case State.Chasing:
                ChaseBehaviour();
                break;
            case State.Attacking:
                AttackBehaviour();
                break;
            case State.Tired:
                TiredBehaviour();
                break;
            case State.RunAway:
                RunAwayBehaviour();
                break;
        }
    }

    private void UpdatePhase()
    {
        //phase 3 if under 50% health
        if (currentHealth <= maxHealth * 0.5f) currentPhase = 3;
        //phase 2 if under 80% health
        else if (currentHealth <= maxHealth * 0.8f) currentPhase = 2;
        //phase 1 from start until under 80% health
        else currentPhase = 1;
    }


    private void IdleBehaviour()
    {
        animator.SetBool("isIdle", true);
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        if (distanceToPlayer <= activationRange)
        {
            currentState = State.Chasing;
            animator.SetBool("isIdle", false);
        }
    }

    private void StartBehaviour()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        if (distanceToPlayer <= activationRange && !startedFight)
        {
            startedFight = true;
            activationRange = 15;
            animator.SetTrigger("BossStart");
            StartCoroutine(WaitForCinematic());
        }
    }

    private IEnumerator WaitForCinematic()
    {
        yield return new WaitForSeconds(8f);
        bossHealthBar.transform.parent.gameObject.SetActive(true);
        currentState = State.Chasing;
        animator.SetBool("isIdle", false);
    }
    private void ChaseBehaviour()
    {
        agent.isStopped = false;
        agent.speed = normalSpeed;
        animator.SetTrigger("Chasing");
        animator.SetBool("isChasing", true);
        agent.SetDestination(player.position);

        // Rotate toward player
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        //if in attack range switch to attacking
        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attacking;
            animator.SetBool("isChasing", false);
        }

    }

    private void AttackBehaviour()
    {
        agent.isStopped = true;

        Vector3 direction = (player.position - transform.position);
        direction.y = 0f; // ignore height difference
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (!isAttacking && attackTimer <= 0f)
        {
            isAttacking = true;
            attackTimer = attackCooldown;
            StartCoroutine(DoAttackCycle());
        }
    }

    //used to ensure the correct attacks are available based on current phase
    private AttackType[] GetAvailableAttacks()
    {
        switch (currentPhase)
        {
            case 1:
                return new AttackType[] { AttackType.Spell };
            case 2:
                return new AttackType[] { AttackType.Spell, AttackType.Minions };
            case 3:
                return new AttackType[] { AttackType.Spell, AttackType.Minions, AttackType.AOE };
            default:
                return new AttackType[] { AttackType.Spell };
        }
    }

    private List<AttackType> GenerateAttackCycle()
    {
        bool minionsAdded = false;
        AttackType[] availableAttacks = GetAvailableAttacks();
        List<AttackType> attackCycle = new List<AttackType>();

        // Add guaranteed new attack
        if (currentPhase == 2)
        {
            attackCycle.Add(AttackType.Minions);
            minionsAdded = true;
        }
        else if (currentPhase == 3) attackCycle.Add(AttackType.AOE);

        while (attackCycle.Count < 3)
        {
            AttackType randomAttack = availableAttacks[Random.Range(0, availableAttacks.Length)];

            if (!(randomAttack == AttackType.Minions && minionsAdded))
            {
                attackCycle.Add(randomAttack);
            }
            if (randomAttack == AttackType.Minions)
            {
                minionsAdded = true;
            }
        }
        return attackCycle;
    }

    private IEnumerator DoAttackCycle()
    {

        List<AttackType> cycle = GenerateAttackCycle();

        foreach (AttackType attack in cycle)
        {
            switch (attack)
            {
                case AttackType.Spell:
                    yield return StartCoroutine(DoSpell());
                    break;
                case AttackType.Minions:
                    yield return StartCoroutine(DoSpawnMinions());
                    break;
                case AttackType.AOE:
                    yield return StartCoroutine(DoAOE());
                    break;
            }
            print(attack);
            yield return new WaitForSeconds(1f);
        }
        //after all attacks enters tired state
        currentState = State.Tired;
        isAttacking = false;
    }


    private IEnumerator DoSpell()
    {
        animator.SetTrigger("Spell");
        Vector3 castPosition = hand.transform.position;

        Vector3 moveDirection = (player.position - castPosition).normalized;
        Quaternion rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(90, 0, 0); ;

        yield return new WaitForSeconds(0.3f);
        GameObject spawnedSpell = Instantiate(spell, castPosition, rotation);
        audioSource.PlayOneShot(spellSound);

        StartCoroutine(MoveSpell(spawnedSpell, moveDirection));
        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator MoveSpell(GameObject spell, Vector3 direction)
    {
        float speed = 7f;
        float duration = 6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            spell.transform.position += direction * speed * Time.deltaTime; // move forward
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(spell); // remove spell after movement

    }
    private IEnumerator DoSpawnMinions()
    {
        animator.SetTrigger("Minions");
        audioSource.PlayOneShot(minionSound);
        int spawnCount = 2;
        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0,minionsSpawnpoints.Length);
            if (!(currentMinionCount >= maxMinionCount))
            {
                currentMinionCount++;
                GameObject minionForBoss = Instantiate(minion, minionsSpawnpoints[randomIndex].position, Quaternion.identity);
                EnemyScript minionScript = minionForBoss.GetComponent<EnemyScript>();
                minionScript.boss = this;
                minionScript.summonedByBoss = true;
            }
        }

        yield return new WaitForSeconds(0.8f);

    }

    private IEnumerator DoAOE()
    {
        animator.SetTrigger("AOE");
        
        Vector3 targetPosition = player.position;
        targetPosition.y--;
        //spawns the indicator for the AOE spell
        GameObject indicator = Instantiate(spellAOEIndicator, targetPosition, Quaternion.identity);

        float warningTime = 1.5f;
        audioSource.PlayOneShot(aoeSound);
        yield return new WaitForSeconds(warningTime);

        Destroy(indicator);
        GameObject spell =  Instantiate(spellAOE,targetPosition, Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        Destroy(spell);

    }



    private void TiredBehaviour()
    {
        if (!isTiredCoroutineRunning)
        {
            shieldActive = false;
            isTiredCoroutineRunning = true;
            hitsTaken = 0;
            agent.isStopped = true;
            animator.SetTrigger("Tired");
            animator.SetBool("isTired", true);
            StartCoroutine(TiredTimer());
        }
    }

    private IEnumerator TiredTimer()
    {
        yield return new WaitForSeconds(5f);
        if (isTiredCoroutineRunning)
        {
            EndTiredState();
        }
    }

    private void EndTiredState()
    {
        isTiredCoroutineRunning = false;
        agent.isStopped = false;
        animator.SetBool("isTired", false);

        Transform furthestPoint = GetFurthestEscapePoint();

        if (furthestPoint != null) runAwayTarget = furthestPoint.position;

        currentState = State.RunAway;

    }

    private void RunAwayBehaviour()
    {
        animator.SetBool("isChasing", true);

        agent.isStopped = false;
        agent.speed = runAwaySpeed;
        agent.SetDestination(runAwayTarget);

        // rotate toward movement direction
        Vector3 direction = (runAwayTarget - transform.position);
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // after running away, chases the player again.
        if (Vector3.Distance(transform.position, runAwayTarget) < 2f)
        {
            StartCoroutine(ActivateShield());
        }
    }

    private IEnumerator ActivateShield()
    {
        if (!shieldAnimationHappened)
        {
            shieldAnimationHappened = true;
            animator.SetTrigger("ShieldAnimation");

            yield return new WaitForSeconds(1.5f);

            shieldActive = true;
            currentState = State.Chasing;
            animator.SetBool("isChasing", false);
            shieldAnimationHappened = false;
        }
    }

    private Transform GetFurthestEscapePoint()
    {
        Transform furthestPoint = null;
        float maxDistance = 0f;

        foreach (Transform point in escapePoints)
        {
            float distance = Vector3.Distance(player.position, point.position);

            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestPoint = point;
            }
        }
        return furthestPoint;
    }




    //damage system
    private void OnTriggerEnter(Collider other)
    {
        if (shieldActive) return;
        if (other.CompareTag("AttackHitboxTag"))
        {
            TakeDamage(1);
        }
        else if (other.CompareTag("SwordAttackHitboxTag"))
        {
            TakeDamage(2);

        }
    }

    private void TakeDamage(int dmg)
    {
        if (!canDamage) return;

        RegisterHitDuringTired(dmg);
        canDamage = false;
        currentHealth -= dmg;
        UpdateHealthBar();
        sparks.SetActive(true);
        audioSource.PlayOneShot(hitSound);
        StartCoroutine(IFramesTime());
        animator.SetTrigger("tookDamage");
        if (currentHealth <= 0 && !isDead)
        {
            StartCoroutine(DeathSequence());
        }
    }


    // Call this whenever the boss takes damage
    private void RegisterHitDuringTired(int hitAmount)
    {
        if (!isTiredCoroutineRunning) return;

        hitsTaken += hitAmount;
        if (hitsTaken >= 3)
        {
            EndTiredState();
        }
    }

    private void UpdateHealthBar()
    {
        bossHealthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    IEnumerator IFramesTime()
    {
        yield return new WaitForSeconds(IFrames);
        sparks.SetActive(false);
        canDamage = true;
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        animator.SetTrigger("Death");
        animator.SetBool("isDead", true);

        agent.isStopped = true;

        gameManager.UpdateScore(1000);

        // Remove remaining summoned enemies
        DestroySummonedMinions();

        yield return new WaitForSeconds(2f);
        gameManager.Win(GameManagerScript.EndingType.GoodEnding);

        Destroy(gameObject);
    }



    private void DestroySummonedMinions()
    {
        EnemyScript[] enemies = FindObjectsOfType<EnemyScript>();

        foreach (EnemyScript enemy in enemies)
        {
            if (enemy.summonedByBoss)
            {
                Destroy(enemy.gameObject);
            }
        }
    }
}
