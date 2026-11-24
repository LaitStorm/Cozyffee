using UnityEngine;

/// <summary>
/// Reproduce un prefab de partículas una sola vez en una posición y lo destruye al terminar,
/// aunque el prefab tenga Looping activado o no se destruya solo.
/// </summary>
public static class OneShotEffect
{
    public static GameObject Play(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        GameObject effect = Object.Instantiate(prefab, position, rotation);
        ParticleSystem[] systems = effect.GetComponentsInChildren<ParticleSystem>();
        float lifetime = 0f;

        foreach (ParticleSystem system in systems)
        {
            // Funciona aunque ya esté reproduciéndose (Play On Awake): termina el ciclo actual y no vuelve a empezar.
            ParticleSystem.MainModule main = system.main;
            main.loop = false;
            lifetime = Mathf.Max(lifetime, main.startDelay.constantMax + main.duration + main.startLifetime.constantMax);

            if (!system.isPlaying)
                system.Play(false);
        }

        Object.Destroy(effect, lifetime > 0f ? lifetime + 0.1f : 5f);
        return effect;
    }

    public static GameObject Play(GameObject prefab, Vector3 position) => Play(prefab, position, Quaternion.identity);
}
