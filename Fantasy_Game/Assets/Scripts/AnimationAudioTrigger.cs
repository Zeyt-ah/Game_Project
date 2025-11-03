using UnityEngine;


[RequireComponent (typeof(AudioSource))]
public class AnimationAudioTrigger : MonoBehaviour
{
    public bool isHuman = false; 
    public AudioClip[] footstepSounds;
    public AudioClip[] runSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] damageSounds;
    public AudioClip deathSound;
    public AudioClip grabSound;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource> ();
    }

    public void LeftFoot()
    {
        int n = Random.Range (1, footstepSounds.Length);
        audioSource.clip = footstepSounds [n];
        audioSource.PlayOneShot(audioSource.clip);

        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = audioSource.clip;
    }

    public void LeftFootRun()
    {
        int n = Random.Range(1, runSounds.Length);
        audioSource.clip = runSounds[n];
        audioSource.PlayOneShot(audioSource.clip);

        runSounds[n] = runSounds[0];
        runSounds[0] = audioSource.clip;
    }

    public void RightFoot()
    {
        int n = Random.Range(1, footstepSounds.Length);
        audioSource.clip = footstepSounds[n];
        audioSource.PlayOneShot(audioSource.clip);

        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = audioSource.clip;
    }
    public void RightFootRun()
    {
        int n = Random.Range(1, runSounds.Length);
        audioSource.clip = runSounds[n];
        audioSource.PlayOneShot(audioSource.clip);

        runSounds[n] = runSounds[0];
        runSounds[0] = audioSource.clip;
    }

    public void Attack()
    {
        int n = Random.Range(1, attackSounds.Length);
        audioSource.clip = attackSounds[n];
        audioSource.PlayOneShot(audioSource.clip);

        attackSounds[n] = attackSounds[0];
        attackSounds[0] = audioSource.clip;
    }

    public void TakeDamage()
    {
        int n = Random.Range(1, damageSounds.Length);
        audioSource.clip = damageSounds[n];
        audioSource.PlayOneShot(audioSource.clip);

        damageSounds[n] = damageSounds[0];
        damageSounds[0] = audioSource.clip;
    }
    public void Death()
    {
        audioSource.clip = deathSound;
        audioSource.PlayOneShot(audioSource.clip);
    }

    public void Grab()
    {
        audioSource.clip = grabSound;
        audioSource.PlayOneShot(audioSource.clip);
    }

}
