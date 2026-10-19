using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField]
    public AudioSource sfxSource;
    public AudioSource dialogueSource;

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip && sfxSource)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void StartDialogueLoop(AudioClip clip, float volume = 1f)
    {
        if (clip && dialogueSource)
        {
            dialogueSource.clip = clip;
            dialogueSource.volume = volume;
            dialogueSource.loop = true;
            dialogueSource.Play();
        }
    }

    public void StopDialogueLoop()
    {
        if (dialogueSource)
        {
            dialogueSource.Stop();
            dialogueSource.loop = false;
        }
    }
}
