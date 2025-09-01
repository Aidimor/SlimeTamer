using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LightBinder : MonoBehaviour
{
    public Light targetLight; // arrastra aquí la luz que quieres usar
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (targetLight != null && rend != null)
        {
            Vector3 pos = targetLight.transform.position;
            Color col = targetLight.color * targetLight.intensity;

            rend.material.SetVector("_LightPos", new Vector4(pos.x, pos.y, pos.z, 1));
            rend.material.SetColor("_LightColor", col);
        }
    }
}
