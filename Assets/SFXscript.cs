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
    public AudioSource _windBFX;
    public float _windSetVolume;
    float _windVolume;



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
    }

    //// Métodos directos si quieres llamar por nombre
    //public void PlaySalto() => PlaySound(_jump);
    //public void PlayDisparo() => PlaySound(_fall);

}
