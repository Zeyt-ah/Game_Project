using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class BossScript : MonoBehaviour
{

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Collider attackCollider;
    public Animator animator;
    public GameManagerScript gameManager;
    public GameObject shieldVisual;

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

    private bool isTiredCoroutineRunning = false;
    private bool isAttacking = false;

    private enum State { Idle, Chasing, Attacking, Tired, Dead }
    private State currentState = State.Idle;

    //for attack types
    private enum AttackType { Spell, Minions, AOE}


    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;
        print(currentHealth);

        UpdatePhase();
        attackTimer -= Time.deltaTime;
        if (shieldVisual) shieldVisual.SetActive(shieldActive);

        switch (currentState)
        {
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
        }
    }

    private void UpdatePhase()
    {
        //phase 3 if under 30% health
        if (currentHealth <= maxHealth * 0.3f) currentPhase = 3;
        //phase 2 if under 70% health
        else if (currentHealth <= maxHealth * 0.7f) currentPhase = 2;
        //phase 1 from start until under 70% health
        else currentPhase = 1;
    }


    //used to ensure the correct attacks are available based on current phase
    private AttackType[] GetAvailableAttacks()
    {
        switch (currentPhase)
        {
            case 1:
                return new AttackType[] {AttackType.Spell };
            case 2:
                return new AttackType[] {AttackType.Spell, AttackType.Minions };
            case 3:
                return new AttackType[] {AttackType.Spell, AttackType.Minions, AttackType.AOE };
            default:
                return new AttackType[] {AttackType.Spell };
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
        // TODO: spawn projectile
        yield return new WaitForSeconds(0.7f);
    }

    private IEnumerator DoSpawnMinions()
    {
        animator.SetTrigger("Minions");
        // TODO: instantiate minions
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator DoAOE()
    {
        animator.SetTrigger("AOE");
        // TODO: send a sky strike esque AOE 
        yield return new WaitForSeconds(1.5f);
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


    private void ChaseBehaviour()
    {
        agent.isStopped = false;
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

        //if player moves too far away back to idle
        if (distanceToPlayer > activationRange * 1.5f)
        {
            currentState = State.Idle;
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
        shieldActive = true;
        agent.isStopped = false;
        animator.SetBool("isTired", false);
        currentState = State.Idle;
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
        StartCoroutine(IFramesTime());
        animator.SetTrigger("tookDamage");
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
    IEnumerator IFramesTime()
    {
        yield return new WaitForSeconds(IFrames);
        canDamage = true;
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        animator.SetBool("isDead", true);
        agent.isStopped = true;
        attackCollider.enabled = false;
        Destroy(gameObject, 10f);
        gameManager.UpdateScore(100);
    }

}
