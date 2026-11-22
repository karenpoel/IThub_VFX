using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    // Хранит исходные количества частиц в burst-ах одного слоя и умножает их на силу.
    // Массивы создаются один раз, поэтому во время игры мусор не образуется.
    private class BurstLayer
    {
        private readonly ParticleSystem system;
        private readonly ParticleSystem.Burst[] bursts;
        private readonly float[] baseCounts;

        public BurstLayer(ParticleSystem system)
        {
            this.system = system;
            ParticleSystem.EmissionModule emission = system.emission;
            bursts = new ParticleSystem.Burst[emission.burstCount];
            emission.GetBursts(bursts);

            baseCounts = new float[bursts.Length];
            for (int i = 0; i < bursts.Length; i++)
                baseCounts[i] = bursts[i].count.constant;
        }

        public void Apply(float scale)
        {
            for (int i = 0; i < bursts.Length; i++)
            {
                float count = Mathf.Max(1f, Mathf.Round(baseCounts[i] * scale));
                bursts[i].count = new ParticleSystem.MinMaxCurve(count);
            }

            ParticleSystem.EmissionModule emission = system.emission;
            emission.SetBursts(bursts);
        }
    }

    [Header("Слои")]
    [SerializeField] private ParticleSystem flash;
    [SerializeField] private ParticleSystem debris;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private Light flashLight;

    [Header("Сила")]
    [SerializeField] private float minStrength = 0.3f;
    [SerializeField] private float maxStrength = 3f;

    [Header("Свет вспышки")]
    [SerializeField] private float lightIntensity = 8f;
    [SerializeField] private float lightRange = 6f;
    [SerializeField] private float lightDuration = 0.25f;

    [Header("Время жизни эффекта (больше, чем самый долгий дым)")]
    [SerializeField] private float totalDuration = 5f;

    [Header("Упрощённый режим для слабого железа")]
    [SerializeField] private int lowQualityLevel = 0;
    [SerializeField] private float lowQualityCountScale = 0.5f;
    [SerializeField] private bool forceLowQuality = false;

    private ParticleSystem[] allSystems;
    private BurstLayer debrisLayer;
    private BurstLayer smokeLayer;
    private float strength = 1f;
    private float age;

    private void Awake()
    {
        allSystems = GetComponentsInChildren<ParticleSystem>(true);
        debrisLayer = new BurstLayer(debris);
        smokeLayer = new BurstLayer(smoke);
        flashLight.enabled = false;
    }

    public void Play(Vector3 position, float power)
    {
        strength = Mathf.Clamp(power, minStrength, maxStrength);
        bool low = forceLowQuality || QualitySettings.GetQualityLevel() <= lowQualityLevel;

        // Размер: масштаб корня увеличивает все слои, потому что у них Scaling Mode = Hierarchy
        transform.SetPositionAndRotation(position, Quaternion.identity);
        transform.localScale = Vector3.one * strength;

        // Количество частиц: умножаем счётчики burst-ов
        float countScale = strength * (low ? lowQualityCountScale : 1f);
        debrisLayer.Apply(countScale);
        smokeLayer.Apply(countScale);

        // На слабом железе выключаем трейлы и свет
        ParticleSystem.TrailModule trails = debris.trails;
        trails.enabled = !low;

        gameObject.SetActive(true);

        for (int i = 0; i < allSystems.Length; i++)
        {
            allSystems[i].Clear(false);
            allSystems[i].Play(false);
        }

        age = 0f;
        flashLight.enabled = !low;
        flashLight.range = lightRange * strength;
        flashLight.intensity = lightIntensity * Mathf.Sqrt(strength);
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (flashLight.enabled)
        {
            float t = age / lightDuration;
            if (t >= 1f)
            {
                flashLight.enabled = false;
            }
            else
            {
                float falloff = (1f - t) * (1f - t);
                flashLight.intensity = lightIntensity * Mathf.Sqrt(strength) * falloff;
            }
        }

        if (age >= totalDuration)
            gameObject.SetActive(false);
    }
}
