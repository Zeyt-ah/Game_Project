using System.Collections;
using UnityEngine;

public class BossIntroSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DragonBossController dragonBossController;
    [SerializeField] private Transform dragonTransform;
    [SerializeField] private Transform wizardTransform;
    [SerializeField] private Transform wizardCarryPoint;
    [SerializeField] private Transform perchPoint;
    [SerializeField] private Transform landingPoint;

    [Header("Movement")]
    [SerializeField] private float flyToWizardTime = 3f;
    [SerializeField] private float flyToPerchSpeed = 18f;
    [SerializeField] private float flyToLandingTime = 3f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float perchDistance = 2f;

    [Header("Path Heights")]
    [SerializeField] private float wizardGrabHeight = 6f;
    [SerializeField] private float wizardSwoopHeight = 35f;
    [SerializeField] private float perchApproachHeight = 30f;
    [SerializeField] private float perchApproachDistance = 20f;

    [Header("Landing Fix")]
    [SerializeField] private float landingGroundOffset = 0f;
    [SerializeField] private float landingSwoopHeight = 30f;
    [SerializeField] private float landingForwardOffset = 12f;

    [Header("Landing Animation Fix")]
    [SerializeField] private float flyingLandingHeightOffset = 2f;
    [SerializeField] private float landingSettleTime = 0.35f;

    [Header("Timing")]
    [SerializeField] private float waitBeforeGrab = 0.5f;
    [SerializeField] private float waitAfterGrab = 0.5f;
    [SerializeField] private float battleStanceTime = 2f;
    [SerializeField] private float waitBeforeFight = 0.5f;

    [Header("Wizard Settings")]
    [SerializeField] private bool hideWizardAtPerch = true;


    private bool introRunning = false;


    // Starts the intro sequence after the final wizard dialogue option is clicked
    public void StartBossIntro()
    {
        if (introRunning)
        {
            return;
        }

        StartCoroutine(BossIntroRoutine());
    }


    // Handles the dragon picking up the wizard, taking him to the perch, starting fight
    private IEnumerator BossIntroRoutine()
    {
        introRunning = true;

        if (dragonBossController == null || wizardTransform == null || wizardCarryPoint == null || perchPoint == null || landingPoint == null)
        {
            Debug.LogWarning("BossIntroSequence is missing one or more references.");
            introRunning = false;
            yield break;
        }

        dragonTransform = dragonBossController.transform;

        dragonBossController.PauseDragonBehaviour();

        yield return new WaitForSeconds(waitBeforeGrab);

        yield return FlyDragonToWizard();

        AttachWizardToDragon();

        yield return new WaitForSeconds(waitAfterGrab);

        yield return FlyDragonToPerch();

        RemoveWizardAtPerch();
        dragonBossController.PlayBattleStance();

        yield return new WaitForSeconds(battleStanceTime);

        dragonBossController.PlayFlyingAnimation();

        yield return FlyDragonToLandingPoint();

        yield return new WaitForSeconds(waitBeforeFight);

        dragonBossController.StartGroundBossFight();

        introRunning = false;
    }

    // Moves the dragon down to the wizard before grabbing him
    private IEnumerator FlyDragonToWizard()
    {
        Vector3 startPosition = dragonTransform.position;
        Vector3 endPosition = wizardTransform.position + Vector3.up * wizardGrabHeight;

        Vector3 controlPoint = (startPosition + endPosition) * 0.5f;
        controlPoint.y = Mathf.Max(startPosition.y, endPosition.y) + wizardSwoopHeight;

        float elapsedTime = 0f;

        while (elapsedTime < flyToWizardTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / flyToWizardTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 nextPosition = CalculateQuadraticBezierPoint(
                smoothT,
                startPosition,
                controlPoint,
                endPosition
            );

            RotateDragonFlatTowards(nextPosition);

            dragonTransform.position = nextPosition;

            yield return null;
        }

        dragonTransform.position = endPosition;

        Debug.Log("Dragon reached the wizard.");
    }

    // Parents the wizard to the dragon carry point so it looks like he is being carried
    private void AttachWizardToDragon()
    {
        Collider wizardCollider = wizardTransform.GetComponent<Collider>();

        if (wizardCollider != null)
        {
            wizardCollider.enabled = false;
        }

        NPCSystem npcSystem = wizardTransform.GetComponent<NPCSystem>();

        if (npcSystem != null)
        {
            npcSystem.enabled = false;
        }

        wizardTransform.SetParent(wizardCarryPoint);
        wizardTransform.localPosition = Vector3.zero;
        wizardTransform.localRotation = Quaternion.identity;

        Debug.Log("Wizard attached to dragon.");
    }

    // Moves the dragon to the perch point while carrying the wizard
    private IEnumerator FlyDragonToPerch()
    {
        Vector3 approachOffset = -perchPoint.forward * perchApproachDistance + Vector3.up * perchApproachHeight;
        Vector3 highApproachPoint = perchPoint.position + approachOffset;

        while (Vector3.Distance(dragonTransform.position, highApproachPoint) > perchDistance)
        {
            MoveDragonTowardsFlat(highApproachPoint, flyToPerchSpeed);
            yield return null;
        }

        Vector3 descendStart = dragonTransform.position;
        Vector3 descendEnd = perchPoint.position;

        float descendTime = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < descendTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / descendTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 nextPosition = Vector3.Lerp(descendStart, descendEnd, smoothT);

            dragonTransform.rotation = Quaternion.Slerp(
                dragonTransform.rotation,
                perchPoint.rotation,
                rotationSpeed * Time.deltaTime
            );

            dragonTransform.position = nextPosition;

            yield return null;
        }

        dragonTransform.position = perchPoint.position;
        dragonTransform.rotation = perchPoint.rotation;

        Debug.Log("Dragon reached the perch.");
    }

    // Deactivates or detaches the wizard once the dragon reaches the perch
    private void RemoveWizardAtPerch()
    {
        wizardTransform.SetParent(null);

        if (hideWizardAtPerch)
        {
            wizardTransform.gameObject.SetActive(false);
        }

        Debug.Log("Wizard removed at perch.");
    }

    // Moves the dragon from the perch to the arena floor without dipping below the landing height
    private IEnumerator FlyDragonToLandingPoint()
    {
        if (dragonBossController == null || landingPoint == null)
        {
            yield break;
        }

        Transform dragonRoot = dragonBossController.transform;

        dragonBossController.PlayFlyingAnimation();

        Vector3 startPosition = dragonRoot.position;

        Vector3 groundLandingPosition = landingPoint.position + Vector3.up * landingGroundOffset;

        Vector3 flyingEndPosition = groundLandingPosition + Vector3.up * flyingLandingHeightOffset;

        Vector3 flatDirectionToLanding = flyingEndPosition - startPosition;
        flatDirectionToLanding.y = 0f;

        if (flatDirectionToLanding.sqrMagnitude <= 0.01f)
        {
            flatDirectionToLanding = dragonRoot.forward;
        }

        flatDirectionToLanding.Normalize();

        Vector3 controlPoint = startPosition;
        controlPoint += flatDirectionToLanding * landingForwardOffset;
        controlPoint += Vector3.up * landingSwoopHeight;

        float elapsedTime = 0f;

        while (elapsedTime < flyToLandingTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / flyToLandingTime);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 nextPosition = CalculateQuadraticBezierPoint(
                smoothT,
                startPosition,
                controlPoint,
                flyingEndPosition
            );

            if (nextPosition.y < flyingEndPosition.y)
            {
                nextPosition.y = flyingEndPosition.y;
            }

            dragonRoot.position = nextPosition;

            Vector3 lookDirection = flyingEndPosition - dragonRoot.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);

                dragonRoot.rotation = Quaternion.Slerp(
                    dragonRoot.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        dragonRoot.position = flyingEndPosition;

        Vector3 finalLookDirection = landingPoint.forward;
        finalLookDirection.y = 0f;

        if (finalLookDirection.sqrMagnitude > 0.01f)
        {
            dragonRoot.rotation = Quaternion.LookRotation(finalLookDirection.normalized);
        }

        dragonBossController.PlayBattleStance();

        yield return SettleDragonOntoGround(flyingEndPosition, groundLandingPosition);

        Debug.Log("Dragon settled onto ground. Root Y: " + dragonRoot.position.y);
    }

    // Lowers the dragon from its flying pose height to its real ground position
    private IEnumerator SettleDragonOntoGround(Vector3 startPosition, Vector3 endPosition)
    {
        Transform dragonRoot = dragonBossController.transform;

        float elapsedTime = 0f;

        while (elapsedTime < landingSettleTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / landingSettleTime);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            dragonRoot.position = Vector3.Lerp(startPosition, endPosition, smoothT);

            yield return null;
        }

        dragonRoot.position = endPosition;
    }

    // Moves the dragon towards a target while rotating mostly horizontally
    private void MoveDragonTowardsFlat(Vector3 targetPosition, float speed)
    {
        RotateDragonFlatTowards(targetPosition);

        dragonTransform.position = Vector3.MoveTowards(
            dragonTransform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    // Rotates the dragon horizontally towards a target position
    private void RotateDragonFlatTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - dragonTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        dragonTransform.rotation = Quaternion.Slerp(
            dragonTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }


    // Calculates a curved point between start, control, and end positions
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 start, Vector3 control, Vector3 end)
    {
        float oneMinusT = 1f - t;

        return
            oneMinusT * oneMinusT * start +
            2f * oneMinusT * t * control +
            t * t * end;
    }

    
}