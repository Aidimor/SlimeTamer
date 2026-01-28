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
    public AudioClip _slimeJumping;
    public AudioClip _successSound;
    public AudioClip _failSound;
    public AudioClip _chooseElement;
    public AudioClip _slimeCharge;
    public AudioClip _slimeRelease;
    public AudioClip _slimeArrives;
    public AudioClip _slimeDead;
    public AudioClip _flameOn;
    public AudioClip _melting;
    public AudioClip _platform;
    public AudioClip _dissapearing;
    public AudioClip _ice;
    public AudioClip _comicFlip;
    public AudioClip _co2;
    public AudioClip _magnetism;
    public AudioClip _iron;
    public AudioClip _snowMoving;
    public AudioClip _electricity;
    public AudioClip _c02Move;


    public AudioClip _stickyMudSound;
    public AudioClip _bossAttack;
    public AudioClip _frozen;
    public AudioClip _cut;
    public AudioClip _newElement;
    public AudioClip _boosDamaged;
    public AudioSource _windBFX;
    public float _windSetVolume;
    public AudioSource _strongWind;
    public float _strongWindSetVolume;
    public AudioSource _rainBFX;
    public float _rainSetVolume;
    public AudioSource _fireBFX;
    public float _fireSetVolume;

    public AudioSource _chargeAttack;
    public float _chargeAttackVolume;
    public float _chargeAttackPitch;



    public static SFXscript Instance;

    private void Awake()
    {
         audioSource = GetComponent<AudioSource>();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


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
        _strongWind.volume = Mathf.Lerp(_strongWind.volume, _strongWindSetVolume, 2 * Time.deltaTime);
        _chargeAttack.volume = Mathf.Lerp(_chargeAttack.volume, _chargeAttackVolume, 2 * Time.deltaTime);
        _chargeAttack.pitch = Mathf.Lerp(_chargeAttack.pitch, _chargeAttackPitch, 2 * Time.deltaTime);
    }
}
