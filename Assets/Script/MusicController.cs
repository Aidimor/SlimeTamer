using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource _audioBGM;
    public AudioClip[] _allThemes;

    void Awake()
    {
        _audioBGM = GetComponent<AudioSource>();
        _audioBGM.loop = true; // opcional pero común en BGM
    }

    public void PlayMusic(int themeIndex)
    {
        if (themeIndex < 0 || themeIndex >= _allThemes.Length)
        {
            Debug.LogWarning("Índice de música fuera de rango");
            return;
        }

        AudioClip newClip = _allThemes[themeIndex];

        //  Si ya es el mismo clip, NO lo reinicies
        if (_audioBGM.clip == newClip)
        {
            if (!_audioBGM.isPlaying)
                _audioBGM.Play();

            return;
        }

        // 👉 Solo si es diferente
        _audioBGM.clip = newClip;
        _audioBGM.Play();
    }
}
