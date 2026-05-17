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

    [Header("Timing")]
    [SerializeField] private float waitBeforeGrab = 1f;
    [SerializeField] private float waitBeforeFight = 2f;


    // Starts the intro sequence after the final wizard dialogue option is clicked
    public void StartBossIntro()
    {
        StartCoroutine(BossIntroRoutine());
    }

    // Handles the dragon picking up the wizard, taking him to the perch, starting fight
    private IEnumerator BossIntroRoutine()
    {
        if (dragonBossController == null || wizardTransform == null)
        {
            Debug.LogWarning("BossIntroSequence is missing references.");
            yield break;
        }

        yield return new WaitForSeconds(waitBeforeGrab);

        AttachWizardToDragon();

        dragonBossController.SendDragonToPerch();

        yield return new WaitUntil(IsDragonAtPerch);

        DetachWizardAtPerch();

        yield return new WaitForSeconds(waitBeforeFight);

        dragonBossController.StartBossFight();
    }

    // Parents the wizard to the dragon carry point so it looks like he is being carried
    private void AttachWizardToDragon()
    {
        if (wizardCarryPoint == null)
        {
            Debug.LogWarning("WizardCarryPoint is missing.");
            return;
        }

        wizardTransform.SetParent(wizardCarryPoint);
        wizardTransform.localPosition = Vector3.zero;
        wizardTransform.localRotation = Quaternion.identity;

        Debug.Log("Wizard attached to dragon.");
    }

    // Checks whether the dragon has reached the perch point
    private bool IsDragonAtPerch()
    {
        if (dragonTransform == null || perchPoint == null)
        {
            return true;
        }

        float distance = Vector3.Distance(dragonTransform.position, perchPoint.position);
        return distance < 2f;
    }

    // Detaches or hides the wizard once the dragon reaches the perch
    private void DetachWizardAtPerch()
    {
        wizardTransform.gameObject.SetActive(false);
        Debug.Log("Wizard removed at perch.");
    }
}