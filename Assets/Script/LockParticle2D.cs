using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ParticleSystem))]
public class ForceField2D : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
    public float fuerza = 5f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        // 🔹 Intentar una vez al inicio
        TryAssignMainScript(SceneManager.GetActiveScene());

        // 🔹 Suscribirse al evento para detectar cambios de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 🔹 Evitar duplicar suscripciones si el objeto se destruye
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAssignMainScript(scene);
    }

    private void TryAssignMainScript(Scene scene)
    {
        // Solo buscar si estamos en la escena "MainGame"
        if (scene.name == "MainGame")
        {
            GameObject go = GameObject.Find("MainGameplayScript");
            if (go != null)
            {
                _scriptMain = go.GetComponent<MainGameplayScript>();
                Debug.Log("✅ ForceField2D: MainGameplayScript encontrado en escena " + scene.name);
            }
            else
            {
                Debug.LogWarning("⚠️ ForceField2D: No se encontró 'MainGamePlayScript' en escena " + scene.name);
            }
        }
        else
        {
            _scriptMain = null;
        }
    }

    //void LateUpdate()
    //{
    //    if (_scriptMain == null)
    //        return;

    //    if (particles == null || particles.Length < ps.main.maxParticles)
    //        particles = new ParticleSystem.Particle[ps.main.maxParticles];

    //    int count = ps.GetParticles(particles);

    //    for (int i = 0; i < count; i++)
    //    {
    //        Vector3 dir = (particles[i].position - _scriptMain._windBlocker.transform.position).normalized;
    //        dir.z = 0;
    //        particles[i].velocity += dir * fuerza * Time.deltaTime;
    //        particles[i].position = new Vector3(particles[i].position.x, particles[i].position.y, 0);
    //    }

    //    ps.SetParticles(particles, count);
    //}
}
