using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CellAntManager : MonoBehaviour
{
    public static CellAntManager Instance;

    public GameObject CellAntProto;
    ObjectPool<ModCellFollower> pool;
    List<ModCellFollower> active = new List<ModCellFollower>(200);

    const int DefaultItems = 200;
    const int MaxItems = 1000;

    private void Awake()
    {
        Instance = this;

        pool = new ObjectPool<ModCellFollower>(
            createFunc: Create,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyCellAnt,
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
            var cellAnt = Get();
            cellAnt.gameObject.SetActive(false);
        }

        Reset();
    }

    ModCellFollower Create()
    {
        var ant = Instantiate(CellAntProto, Vector3.one * 12851, Quaternion.identity, this.gameObject.transform);
        var mod = ant.GetComponent<ModCellFollower>();
        return mod;
    }

    void OnGet(ModCellFollower ant)
    {
        ant.Reset();
        active.Add(ant);
    }

    void OnRelease(ModCellFollower ant)
    {
        ant.gameObject.SetActive(false);
        active.Remove(ant);
    }

    void OnDestroyCellAnt(ModCellFollower ant)
    {
        Destroy(ant.gameObject);
        active.Remove(ant);
    }

    public ModCellFollower Get() => pool.Get();
    public void Release(ModCellFollower cellAnt) => pool.Release(cellAnt);

    public void Reset()
    {
        while (active.Count > 0)
            Release(active[active.Count - 1]);
    }

    public void EngineTick()
    {
        for (int i = 0; i < active.Count; ++i)
            active[i].EngineTick();
    }
}
