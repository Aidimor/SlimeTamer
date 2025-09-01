using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ForceField2D : MonoBehaviour
{
    public Transform objeto;   // El objeto alrededor del cual se repelen
    public float fuerza = 5f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        objeto = GameObject.Find("World/SlimeBase").GetComponent<SlimeController>()._WindBlocker.transform;
    }

    void LateUpdate()
    {
        if (particles == null || particles.Length < ps.main.maxParticles)
            particles = new ParticleSystem.Particle[ps.main.maxParticles];

        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // Calcular dirección desde el objeto hasta la partícula
            Vector3 dir = (particles[i].position - objeto.position).normalized;
            dir.z = 0; // 🔹 Forzar que solo actúe en XY

            // Aplicar fuerza (como velocidad)
            particles[i].velocity += dir * fuerza * Time.deltaTime;

            // Mantener en el plano Z=0
            particles[i].position = new Vector3(particles[i].position.x, particles[i].position.y, 0);
        }

        ps.SetParticles(particles, count);
    }
}
