using System.Collections;
using UnityEngine;

public class DragonBossController : MonoBehaviour
{
    private enum DragonState
    {
        Circling,
        FlyingToPerch,
        Perched,
        BossFight
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform perchPoint;
    [SerializeField] private Transform[] flightPoints;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float circleSpeed = 12f;
    [SerializeField] private float perchSpeed = 10f;
    [SerializeField] private float attackSpeed = 16f;
    [SerializeField] private float rotationSpeed = 4f;
    [SerializeField] private float reachDistance = 1.5f;

    [Header("Boss Fight Settings")]
    [SerializeField] private float attackHeight = 8f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float attackDistance = 4f;

    [Header("Orbit Settings")]
    [SerializeField] private Transform orbitCentre;
    [SerializeField] private float orbitRadiusX = 95f;
    [SerializeField] private float orbitRadiusZ = 110f;
    [SerializeField] private float orbitHeight = 40f;
    [SerializeField] private float orbitSpeed = 0.25f;

    private float orbitAngle;

    private DragonState currentState = DragonState.Circling;
    private int currentFlightPointIndex = 0;
    private bool canAttack = true;

    private void Start()
    {
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

        PlayFlyingAnimation();
    }

    private void Update()
    {
        switch (currentState)
        {
            case DragonState.Circling:
                CircleArena();
                break;

            case DragonState.FlyingToPerch:
                FlyToPerch();
                break;

            case DragonState.Perched:
                StayPerched();
                break;

            case DragonState.BossFight:
                BossFightBehaviour();
                break;
        }
    }

    // Moves the dragon smoothly above the arena
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

        MoveTowards(targetPosition, circleSpeed);
    }

    // Sends the dragon to the tower perch after the wizard interaction
    private void FlyToPerch()
    {
        if (perchPoint == null)
        {
            return;
        }

        MoveTowards(perchPoint.position, perchSpeed);

        float distance = Vector3.Distance(transform.position, perchPoint.position);

        if (distance <= reachDistance)
        {
            transform.position = perchPoint.position;
            transform.rotation = perchPoint.rotation;

            currentState = DragonState.Perched;
            PlayIdleAnimation();
        }
    }

    // Keeps the dragon still on the tower until the boss fight starts
    private void StayPerched()
    {
        if (perchPoint == null)
        {
            return;
        }

        transform.position = perchPoint.position;
        transform.rotation = perchPoint.rotation;
    }

    // Controls the dragon after the boss fight begins
    private void BossFightBehaviour()
    {
        if (player == null)
        {
            return;
        }

        Vector3 attackTarget = player.position + Vector3.up * attackHeight;

        MoveTowards(attackTarget, attackSpeed);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackDistance && canAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // Moves and rotates the dragon towards a target position
    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    // Called by the wizard dialogue when the dragon should perch
    public void SendDragonToPerch()
    {
        currentState = DragonState.FlyingToPerch;
        PlayFlyingAnimation();
    }

    // Called when the boss fight starts
    public void StartBossFight()
    {
        currentState = DragonState.BossFight;
        PlayFlyingAnimation();
    }

    // Handles attack timing
    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        PlayAttackAnimation();

        Debug.Log("Dragon attacks the player.");

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        PlayFlyingAnimation();
    }

    // Plays the flying animation
    private void PlayFlyingAnimation()
    {
        if (animator != null)
        {
            animator.Play("FlyingFWD");
        }
    }

    // Plays the perched/idle animation
    private void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.Play("IdleSimple");
        }
    }

    // Plays the attack animation
    private void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.Play("FlyingAttack");
        }
    }
}