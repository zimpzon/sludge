using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance;

    public GameObject BulletProto;
    ObjectPool<ModBulletMovement> pool;
    List<ModBulletMovement> activeBullets = new List<ModBulletMovement>(100);

    const int DefaultItems = 50;
    const int MaxItems = 200;

    public void Awake()
    {
        Instance = this;

        pool = new ObjectPool<ModBulletMovement>(
            createFunc: Create,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: false,
            defaultCapacity: MaxItems,
            maxSize: MaxItems);
    }

    private void Start()
    {
        WarmupPool();
    }

    void WarmupPool()
    {
        for (int i = 0; i < DefaultItems; ++i)
        {
            var bullet = Get();
            bullet.gameObject.SetActive(false);
        }

        Reset();
    }

    ModBulletMovement Create()
    {
        return Instantiate(BulletProto, Vector3.zero, Quaternion.identity, this.gameObject.transform).GetComponent<ModBulletMovement>();
    }

    void OnGet(ModBulletMovement bullet)
    {
        bullet.Reset();
        bullet.gameObject.SetActive(true);
        activeBullets.Add(bullet);
    }

    void OnRelease(ModBulletMovement bullet)
    {
        bullet.gameObject.SetActive(false);
        activeBullets.Remove(bullet);
    }

    void OnDestroyBullet(ModBulletMovement bullet)
    {
        Destroy(bullet.gameObject);
        activeBullets.Remove(bullet);
    }

    public ModBulletMovement Get() => pool.Get();
    public void Release(ModBulletMovement bullet) => pool.Release(bullet);

    public void Reset()
    {
        while (activeBullets.Count > 0)
            Release(activeBullets[activeBullets.Count - 1]);
    }

    public void EngineTick()
    {
        for (int i = 0; i < activeBullets.Count; ++i)
            activeBullets[i].EngineTick();
    }
}
