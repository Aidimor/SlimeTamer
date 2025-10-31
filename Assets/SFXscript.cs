using UnityEngine;

public class SFXscript : MonoBehaviour
{

    private AudioSource audioSource;

    [Header("Clips de Efectos")]
    public AudioClip _jump;
    public AudioClip _fall;
    public AudioClip _explosion;
    public AudioClip _stoneSlide;
    public AudioClip _stoneClose;
    public AudioClip _next;
    public AudioClip _roar;
    public AudioClip _scream;
    public AudioClip _whip;
    public AudioClip _ding;
    public AudioClip _falling;
    public AudioClip _slimeJumping;
    public AudioClip _successSound;
    public AudioClip _failSound;
    public AudioClip _chooseElement;
    public AudioClip _slimeCharge;
    public AudioClip _slimeRelease;
    public AudioClip _slimeArrives;
    public AudioClip _shopEnter;
    public AudioClip _slimeDead;
    public AudioClip _flameOn;
    public AudioClip _rockSlimeMoving;
    public AudioClip _stickyMudSound;
    public AudioSource _windBFX;
    public float _windSetVolume;
    public AudioSource _rainBFX;
    public float _rainSetVolume;
    public AudioSource _fireBFX;
    public float _fireSetVolume;



    private void Awake()
    {
 

        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Reproduce un clip de sonido
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void Update()
    {
        _windBFX.volume = Mathf.Lerp(_windBFX.volume, _windSetVolume, 2 * Time.deltaTime);
        _rainBFX.volume = Mathf.Lerp(_rainBFX.volume, _rainSetVolume, 2 * Time.deltaTime);
        _fireBFX.volume = Mathf.Lerp(_fireBFX.volume, _fireSetVolume, 2 * Time.deltaTime);
    }

    //// Métodos directos si quieres llamar por nombre
    //public void PlaySalto() => PlaySound(_jump);
    //public void PlayDisparo() => PlaySound(_fall);

}
