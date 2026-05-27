using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
{
    private float lastMovePlayTime;
    private float lastMatchPlayTime;

    private AudioSource bgmSource;
    private AudioSource seSource;

    [Header("Settings")]
    [SerializeField] private float moveVolumeScale = 1f;
    [SerializeField] private float moveVolumeScaleDuringMatch = 0.35f;
    [SerializeField] private float matchVolumeScale = 1f;
    [SerializeField] private float bombVolumeScale = 1.25f;
    [SerializeField] private float moveCooldown = 0.08f;
    [SerializeField] private float blockMoveAfterMatchDuration = 0.12f;

    [Header("Clips")]
    [SerializeField] private AudioClip blockSelectClip;
    [SerializeField] private AudioClip blockDeselectClip;
    [SerializeField] private AudioClip blockMoveClip;
    [SerializeField] private AudioClip blockMatchClip;
    [SerializeField] private AudioClip bombBlockClip;
    [SerializeField] private AudioClip feverClip;

    protected override void Awake()
    {
        base.Awake();

        bgmSource = transform.Find("BGM").GetComponent<AudioSource>();
        seSource = transform.Find("SE").GetComponent<AudioSource>();
    }

    public void PlayBlockSelectSFX() => seSource.PlayOneShot(blockSelectClip);

    public void PlayBlockDeselectSFX() => seSource.PlayOneShot(blockDeselectClip);

    public void PlayBlockMoveSFX()
    {
        float nowTime = Time.time;
        float movePlayTimePassed = nowTime - lastMovePlayTime;

        if (movePlayTimePassed < moveCooldown) return;

        lastMovePlayTime = nowTime;

        float timeSinceLastMatch = nowTime - lastMatchPlayTime;
        bool isDuringMatch = timeSinceLastMatch < blockMoveAfterMatchDuration;

        float volumeScale = isDuringMatch ? 
            moveVolumeScaleDuringMatch : moveVolumeScale;

        seSource.PlayOneShot(blockMoveClip, volumeScale);
    }

    public void PlayBlockMatchSFX(bool isBomb)
    {
        lastMatchPlayTime = Time.time;

        AudioClip clip = isBomb ? bombBlockClip : blockMatchClip;
        float volumeScale = isBomb ? bombVolumeScale : matchVolumeScale;

        seSource.PlayOneShot(clip, volumeScale);
    }

    public void PlayFeverSFX() => seSource.PlayOneShot(feverClip);

    public void SetBGMPitch(Mode mode)
    {
        float bgmPitch = mode switch
        {
            Mode.Fever => 1.15f,
            Mode.SuperFever => 1.3f,
            _ => 1f
        };
        bgmSource.pitch = bgmPitch;
    }
}
