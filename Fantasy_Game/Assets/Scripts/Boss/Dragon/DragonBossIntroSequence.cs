using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossIntroSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DragonBossController dragonBossController;
    [SerializeField] private Transform dragonTransform;
    [SerializeField] private Transform wizardTransform;
    [SerializeField] private Transform wizardCarryPoint;
    [SerializeField] private Transform perchPoint;

    [Header("Dragon Movement")]
    [SerializeField] private float flyDownSpeed = 18f;
    [SerializeField] private float flyToPerchSpeed = 18f;
    [SerializeField] private float rotationSpeed = 4f;
    [SerializeField] private float grabDistance = 2f;
    [SerializeField] private float perchDistance = 2f;

    [Header("Timing")]
    [SerializeField] private float waitBeforeGrab = 0.5f;
    [SerializeField] private float waitAfterGrab = 0.5f;
    [SerializeField] private float waitBeforeFight = 1.5f;

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

        if (dragonBossController == null || dragonTransform == null || wizardTransform == null || wizardCarryPoint == null || perchPoint == null)
        {
            Debug.LogWarning("BossIntroSequence is missing one or more references.");
            yield break;
        }

        dragonBossController.enabled = false;

        yield return new WaitForSeconds(waitBeforeGrab);

        yield return FlyDragonToWizard();

        AttachWizardToDragon();

        yield return new WaitForSeconds(waitAfterGrab);

        yield return FlyDragonToPerch();

        RemoveWizardAtPerch();

        yield return new WaitForSeconds(waitBeforeFight);

        dragonBossController.enabled = true;
        dragonBossController.StartBossFight();

        introRunning = false;
    }

    // Moves the dragon down to the wizard before grabbing him
    private IEnumerator FlyDragonToWizard()
    {
        Vector3 startPosition = dragonTransform.position;

        Vector3 endPosition = wizardTransform.position + Vector3.up * 7f;

        Vector3 controlPoint = (startPosition + endPosition) * 0.5f;
        controlPoint.y = Mathf.Max(startPosition.y, endPosition.y) + 25f;

        float journeyTime = 2.5f;
        float elapsedTime = 0f;

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / journeyTime;

            // Smooths the motion so the dragon eases in and out instead of moving mechanically
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 nextPosition = CalculateQuadraticBezierPoint(
                smoothT,
                startPosition,
                controlPoint,
                endPosition
            );

            RotateDragonTowards(nextPosition);

            dragonTransform.position = nextPosition;

            yield return null;
        }

        dragonTransform.position = endPosition;

        Debug.Log("Dragon swooped down to the wizard.");
    }

    // Calculates a curved point between start, control, and end position
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 start, Vector3 control, Vector3 end)
    {
        float oneMinusT = 1f - t;

        return
            oneMinusT * oneMinusT * start +
            2f * oneMinusT * t * control +
            t * t * end;
    }

    // Rotates the dragon towards the next movement position
    private void RotateDragonTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - dragonTransform.position;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        // Reduce rotation so the dragon turns mostly horizontally instead of nose-diving
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);

        if (flatDirection.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized);

        dragonTransform.rotation = Quaternion.Slerp(
            dragonTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
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
        Vector3 startPosition = dragonTransform.position;

        // Go above the perch first so the dragon does not fly directly through the tower
        Vector3 highApproachPoint = perchPoint.position + Vector3.up * 25f;

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

            // Rotate towards the final perch rotation while descending
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

    // Moves the dragon towards a target while rotating mostly horizontally, preventing awkward nose-dives
    private void MoveDragonTowardsFlat(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - dragonTransform.position;

        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);

        if (flatDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized);

            dragonTransform.rotation = Quaternion.Slerp(
                dragonTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        dragonTransform.position = Vector3.MoveTowards(
            dragonTransform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    // Deactivates or detaches the wizard once the dragon reaches the perch
    private void RemoveWizardAtPerch()
    {
        wizardTransform.SetParent(null);
        wizardTransform.gameObject.SetActive(false);
        Debug.Log("Wizard removed at perch.");
    }
}