using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource _audioBGM;
    public AudioClip[] _allThemes;

    private AudioClip _currentClip; // Guarda la canción que está sonando

    void Start()
    {
        _audioBGM = GetComponent<AudioSource>();
    }

    // Llamas a esta función para reproducir música
    public void PlayMusic(int themeIndex)
    {
        if (themeIndex < 0 || themeIndex >= _allThemes.Length)
        {
            Debug.LogWarning("El índice de música está fuera de rango.");
            return;
        }

        AudioClip newClip = _allThemes[themeIndex];

        // Si es diferente a la que suena actualmente
        if (_currentClip != newClip)
        {
            _currentClip = newClip;
            _audioBGM.clip = _currentClip;
            _audioBGM.Play();
        }
        else
        {
            // Si ya está sonando, no hagas nada
            if (!_audioBGM.isPlaying)
            {
                // Pero si se detuvo, reprodúcela de nuevo
                _audioBGM.Play();
            }
        }
    }
}
