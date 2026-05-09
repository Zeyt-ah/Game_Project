using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEditor;
using System.Net.Http.Headers;
public class GolemScript : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Collider attackCollider;
    public Animator animator;
    public GameManagerScript gameManager;

    [Header("Stats")]
    public float detectionRange = 10f;
    private float attackRange = 2.7f;
    public float attackCooldown = 3f;
    public int maxHealth = 5;

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

    private enum State { Roaming, Chasing, Attacking, Dead }
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
            case State.Chasing:
                ChaseBehavior(distanceToPlayer);
                break;
            case State.Attacking:
                AttackBehavior(distanceToPlayer);
                break;
        }
    }

    private void RoamBehavior(float distanceToPlayer)
    {
        agent.isStopped = false;
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

        if (distanceToPlayer > attackRange + 1f)
        {
            attackCollider.enabled = false;
            currentState = State.Chasing;
        }
    }

    private IEnumerator DoAttack()
    {
        animator.SetTrigger("Attack");

        animator.SetBool("IsRoaming", false);

        yield return new WaitForSeconds(0.8f); // delay before attack collider turns on
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.1f); // duration collider stays active
        animator.SetBool("IsRoaming", true);
        attackCollider.enabled = false;
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

