using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioManager audioManager;

    [Header("-------------- Audio Source --------------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-------------- Audio Clip --------------")]
    public AudioClip background;
    public AudioClip slash;
    public AudioClip itemPick;
    public AudioClip axeSlash;
    public AudioClip slimeJump;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
        AudioManager audioManager = this.GetComponent<AudioManager>();
        Player_Combat player_Combat = GameManager.Instance.persistentObjects[1].GetComponent<Player_Combat>();

        player_Combat.audioManager = audioManager;
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
