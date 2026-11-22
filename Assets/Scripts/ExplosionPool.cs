using UnityEngine;

public class ExplosionPool : MonoBehaviour
{
    public static ExplosionPool Instance { get; private set; }

    [SerializeField] private ExplosionEffect prefab;
    [SerializeField] private int poolSize = 8;

    private ExplosionEffect[] pool;
    private int next;

    private void Awake()
    {
        Instance = this;

        pool = new ExplosionEffect[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(prefab, transform);
            pool[i].gameObject.SetActive(false);
        }
    }

    // Берёт следующий взрыв по кругу и запускает его.
    // Если все заняты, перезапускается самый старый.
    public void Spawn(Vector3 position, float strength)
    {
        ExplosionEffect effect = pool[next];
        next = (next + 1) % pool.Length;
        effect.Play(position, strength);
    }
}
