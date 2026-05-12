using System.Collections;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
public class PriestScript : MonoBehaviour
{

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Collider attackCollider;
    public Animator animator;
    public GameManagerScript gameManager;
    //for spell casts
    public GameObject fireBall;
    public GameObject staff;

    [Header("Stats")]
    public float detectionRange = 20f;
    public float attackRange = 10f;
    public float attackCooldown = 10f;
    public int maxHealth = 1;

    [Header("Roaming")]
    public float roamRadius = 10f;
    private Vector3 startPosition;
    private Vector3 roamTarget;

    private int currentHealth;
    private float attackTimer = 0f;
    private bool isDead = false;
    private float IFrames = 0.5f;
    private bool canDamage = true;
    private bool facingPlayer = false;
    private bool isAttacking = false;

    private enum State { Roaming, Idling, Chasing, Attacking, Dead }
    private State currentState = State.Roaming;


    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        currentHealth = maxHealth;
        SetNewRoamTarget();
    }

    void Update()
    {
        if (isDead) return;


        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Roaming:
                RoamBehavior(distanceToPlayer);
                break;
            case State.Idling:
                IdleBehaviour(distanceToPlayer);
                break;
            case State.Chasing:
                ChaseBehavior(distanceToPlayer);
                break;
            case State.Attacking:
                AttackBehavior(distanceToPlayer);
                break;
        }
    }


    private void IdleBehaviour(float distanceToPlayer)
    {
        agent.isStopped = true;
        animator.SetBool("Idling", true);

        if (distanceToPlayer >= detectionRange && attackTimer <=2f) currentState = State.Roaming;
        if (attackTimer <= 0f)currentState = State.Attacking;
        
    }
    private void RoamBehavior(float distanceToPlayer)
    {
        agent.isStopped = false;
        animator.SetBool("Idling", false);
        animator.SetBool("IsRoaming", true);
        agent.SetDestination(roamTarget);

        if (Vector3.Distance(transform.position, roamTarget) < 1f)
            SetNewRoamTarget();

        if (distanceToPlayer <= detectionRange)
            currentState = State.Chasing;
    }

    private void ChaseBehavior(float distanceToPlayer)
    {
        agent.isStopped = false;
        animator.SetBool("Idling", false);
        animator.SetBool("IsRoaming", true);
        agent.SetDestination(player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attacking;
        }
        else if (distanceToPlayer > detectionRange * 1.5f) // lost player
        {
            currentState = State.Roaming;
            SetNewRoamTarget();
        }
    }

    private void AttackBehavior(float distanceToPlayer)
    {
        if (isAttacking) return;
        agent.isStopped = true;
        Vector3 direction = (player.position - transform.position);

        if (!facingPlayer)
        {
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation; // snap immediately
            }
            facingPlayer = true;
        }

        direction.y = 0f; // ignore height difference
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            StartCoroutine(DoAttack());
        }

        if (distanceToPlayer > attackRange + 1f && !isAttacking)
        {
            attackCollider.enabled = false;
            currentState = State.Chasing;
        }

    }

    private IEnumerator DoAttack()
    {
        isAttacking = true;
        currentState = State.Idling;
        animator.SetTrigger("Attack");

        animator.SetBool("IsRoaming", false);

        yield return new WaitForSeconds(3.5f); // delay before attack collider turns on
        //makes sure a spell isnt cast after death
        if (!isDead)
        {
            CastSpell();
            attackCollider.enabled = true;
        }
        yield return new WaitForSeconds(0.1f); // duration collider stays active
        animator.SetBool("IsRoaming", true);
        isAttacking = false;
        attackCollider.enabled = false;
    }

    private void CastSpell()
    {
        Vector3 castPosition = staff.transform.position;
        GameObject spawnedSpell = Instantiate(fireBall, castPosition, Quaternion.identity);

        Vector3 moveDirection = (player.position - castPosition).normalized;

        StartCoroutine(MoveFireball(spawnedSpell, moveDirection));
    }

    private IEnumerator MoveFireball(GameObject spell, Vector3 direction)
    {
        float speed = 5f;
        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            spell.transform.position += direction * speed * Time.deltaTime; // move forward
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(spell); // remove spell after movement

    }

    private void SetNewRoamTarget()
    {
        Vector3 randomDir = Random.insideUnitSphere * roamRadius;
        randomDir += startPosition;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
        {
            roamTarget = hit.position;
        }
    }

    //damage system
    private void OnTriggerEnter(Collider other)
    {
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

        canDamage = false;
        currentHealth -= dmg;
        StartCoroutine(IFramesTime());
        animator.SetTrigger("IsHit");
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
        DropReward();
        animator.SetTrigger("Death");
        animator.SetBool("Dead", true);
        agent.isStopped = true;
        attackCollider.enabled = false;
        Destroy(gameObject, 10f);
        gameManager.UpdateScore(100);
    }

    private void DropReward()
    {
        Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
        Instantiate(gameManager.heartPrefab, dropPosition, Quaternion.identity);
    }
}

