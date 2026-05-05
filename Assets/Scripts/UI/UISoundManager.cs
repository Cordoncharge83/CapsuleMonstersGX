using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip acceptSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip changeMenuSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayAccept()
    {
        Play(acceptSound);
    }

    public void PlayCancel()
    {
        Play(cancelSound);
    }

    public void PlayChangeMenu()
    {
        Play(changeMenuSound);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}