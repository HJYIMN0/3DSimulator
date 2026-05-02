using UnityEngine;

public class FeedBack_Entity : MonoBehaviour
{
    [Header("Setting FootSteps")]
    [SerializeField] private AudioClip[] footStepsClips;
    [SerializeField] private AudioSource footSource;
    [SerializeField] protected float volumeFootSteps = 0.2f;
    [SerializeField] protected float maxDistanceFootstepSound = 15f;

    [Header("Setting Impact")]
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private AudioSource impactSource;
    [SerializeField] protected float volumeImpact = 1f;
    [SerializeField] protected float maxDistanceImpact = 20f;

    public void Start()
    {
        footSource = gameObject.AddComponent<AudioSource>();
        footSource.maxDistance = maxDistanceFootstepSound;
        footSource.volume = volumeFootSteps;
        footSource.spatialBlend = 1;

        impactSource = gameObject.AddComponent<AudioSource>();
        impactSource.maxDistance = maxDistanceImpact;
        impactSource.volume = volumeImpact;
        impactSource.spatialBlend = 1;
    }

    public virtual void PlayFootstepSound()
    {
        float pitch = Random.Range(1f, 1.2f);

        if (footStepsClips != null && footStepsClips.Length > 0)
        {
            AudioClip clip = footStepsClips[Random.Range(0, footStepsClips.Length)];
            footSource.PlayOneShot(clip);
        }

    }

    public virtual void PlayOnImpact()
    {
        if (footStepsClips == null && footStepsClips.Length == 0) return;

        AudioClip clip = impactClips[Random.Range(0, impactClips.Length)];
        impactSource.PlayOneShot(clip);
    }
}
