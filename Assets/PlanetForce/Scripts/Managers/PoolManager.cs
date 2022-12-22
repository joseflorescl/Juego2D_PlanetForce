using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum CreatePoolMode { Start, FirstGet, Default };

[System.Serializable]
public struct PoolData
{
    public string name; // Automáticamente se colocará el nombre del prefab, en el OnValidate
    public Component prefab; 
        // Este prefab se inicializa en el Inspector.
        // Tener el cuidado de que al hacer drag&drop del prefab desde la ventana Project hacia el Inspector
        // NO se debe hacer directamente desde el prefab sino que desde LA componente del prefab que se quiera elegir.
        //  - Hacer Lock en el Inspector para el objeto "Pool Manager"
        //  - En la carpeta Project seleccionar el prefab: botón derecho, Properties
        //  - Hacer drag&drop de LA componente específica del prefab (ej: MissileController) al elemento prefab de poolData
    public int defaultCapacity;
    public int maxSize;
    public CreatePoolMode createPoolMode;
    public Transform parent; // Puede ser null
}


public class PoolManager : MonoBehaviour
{
    /* Ej de uso del PoolManager:
     * - Crear un gameObject de nombre "PoolManager"
     * - Agregarle como componente este script
     * 
     * - Reemplazar la línea de código: 
     *     EnemyController enemy = Instantiate(enemyPrefab, position, rotation);
     *   por:
     *     EnemyController enemy = PoolManager.Instance.Get(enemyPrefab, position, rotation);
     *     
     * - En este caso la var enemyPrefab está declarada de tipo EnemyController. Para usar este PoolManager 
     *   solamente se necesita que el prefab sea de un tipo que herede de Component, 
         como cualquier script creado por nosotros que hereda de MonoBehaviour -> Behaviour -> Component

     * - Y en vez de hacer un Destroy del objeto, se reemplaza:
     *     Destroy(gameObject);
     *   por:
     *     PoolManager.Instance.Release(gameObject);
     *     
     * - NO es necesario realizar ninguna configuración adicional para que el Pool Manager empiece a funcionar.
     *   Si se desea tener un control más detallado de cuántos objetos tendrá cada pool de cada prefab
     *   y si estos objetos se van a crear todos en el Start o se van a crear de a uno, se puede configurar 
     *   un nuevo elemento en el array poolData               
    */

    public static PoolManager Instance;

    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 100;
    [SerializeField] private bool defaultCreateObjects = false;
    [SerializeField] private bool collectionCheck = true; // Recordar que esto solo es para el Editor, para ver errores en caso de usar mal el Release
    [SerializeField] private Transform defaultParent;
    [SerializeField] private PoolData[] poolData;

    // Cada elemento de este dictionary es un pool para un prefab en particular.    
    // El int corresponde al prefab.gameObject.GetInstanceID()
    // En la funcion Get: necesito saber a que ObjectPool pertenece el prefab. La idea es tener una solucion O(1).
    // Por eso usamos un objeto Dictionary que me asocia el <id del prefab, ObjectPool>    
    Dictionary<int, IObjectPool<Component>> pools;

    // En la funcion Release: necesito saber cual es el ObjectPool al que pertenece el gameObject instanciado de un prefab
    // Por eso usamos un objeto Dictionary que me asocia el <id del gameObject, ObjectPool>    
    Dictionary<int, IObjectPool<Component>> objectPoolLookup;

    // Y además en el Release necesitamos la Component que hay que liberar.
    //  Como al PoolManager lo llamarán con un Release(gameObject), como no podemos hacer el GetComponent 
    //  de una componente que no sabemos cuál es de todas las que tiene el gameObject,
    //  entonces se debe guardar esta relación en otro dictionary:
    // Por eso usamos un objeto Dictionary que me asocia el <id del gameObject, Component creada con el Instantiate>    
    Dictionary<int, Component> componentLookup;

    // Para asociar cuál es la data para crear un pool
    // Por eso usamos un objeto Dictionary que me asocia el <id del prefab, PoolData configurada en el Inspector>    
    Dictionary<int, PoolData> poolDataLookup;

    // Como la función CreateFunc no puede recibir argumentos, cuando se quiera hacer el Instantiate de un nuevo objeto
    // se usarán estar vars
    Component prefabTemp; 
    Transform parentTemp;


    private void OnValidate()
    {
        for (int i = 0; i < poolData.Length; i++)
        {
            if (poolData[i].prefab)
            {
                poolData[i].name = poolData[i].prefab.name;
            }
        }
    }

    private void Awake()
    {
        Instance = this;
        pools = new Dictionary<int, IObjectPool<Component>>();
        objectPoolLookup = new Dictionary<int, IObjectPool<Component>>();
        componentLookup = new Dictionary<int, Component>();
        poolDataLookup = new Dictionary<int, PoolData>();
    }

    private void Start()
    {
        for (int i = 0; i < poolData.Length; i++)
        {
            var poolDataItem = poolData[i];
            int prefabID = poolDataItem.prefab.gameObject.GetInstanceID();
            poolDataLookup[prefabID] = poolDataItem;
            
            if (poolDataItem.createPoolMode == CreatePoolMode.Start)
                CreatePool(poolDataItem.prefab, poolDataItem.defaultCapacity, poolDataItem.maxSize, true, poolDataItem.parent);
        }
    }

    public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {        
        var pool = GetPoolOrCreate(prefab);
        prefabTemp = prefab;
        parentTemp = GetParentByPrefab(prefab);

        // Importante: la función pool.Get PUEDE llamar al método CreateFunc, el cual hará el Instantiate
        //  por lo que se requiere tener seteado los valores de las vars prefabTemp y parentTemp, porque la
        //  función CreateFunc NO puede recibir argumentos.
        var obj = pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        
        return (T)obj;
    }

    public T Get<T>(T prefab, Transform parent) where T : Component
    {
        var pool = GetPoolOrCreate(prefab);
        prefabTemp = prefab;
        parentTemp = parent;
        var obj = pool.Get();
        obj.transform.parent = parent;

        return (T)obj;
    }

    IObjectPool<Component> GetPoolOrCreate(Component prefab)
    {
        int prefabID = prefab.gameObject.GetInstanceID();
        IObjectPool<Component> pool;

        if (!pools.TryGetValue(prefabID, out pool))
        {
            int capacity;
            int size;
            bool createObjects;
            Transform parent;
            // Hay que crear el pool, se valida si tiene data configurada en PoolData:
            if (poolDataLookup.TryGetValue(prefabID, out var poolDataItem))
            {
                capacity = poolDataItem.defaultCapacity;
                size = poolDataItem.maxSize;
                createObjects = poolDataItem.createPoolMode == CreatePoolMode.FirstGet || defaultCreateObjects;
                parent = poolDataItem.parent;
            }
            else
            {
                capacity = defaultCapacity;
                size = maxSize;
                createObjects = defaultCreateObjects;
                parent = defaultParent;
            }

            pool = CreatePool(prefab, capacity, size, createObjects, parent);
        }

        return pool;
    }

    Transform GetParentByPrefab(Component prefab)
    {
        int prefabID = prefab.gameObject.GetInstanceID();

        if (poolDataLookup.TryGetValue(prefabID, out var poolDataItem))
            return poolDataItem.parent;
        else
            return defaultParent;
    }
    
    public bool Release(GameObject obj)
    {
        // Para validar si se está tratando de hacer Release 2 veces de un objeto ya devuelto al pool:
        //  una validación simple es si el obj ya está desactivado:
        if (collectionCheck && !obj.activeInHierarchy)
        {
            print("Release de un objeto ya desactivado");
            return false;
        }

        var gameObjectID = obj.GetInstanceID();
        if (objectPoolLookup.TryGetValue(gameObjectID, out var pool))
        {
            var component = componentLookup[gameObjectID]; // Buscamos la componente a liberar
            pool.Release(component);
            return true;
        }
        else
        {
            print("Error al usar el Pool Manager: se quiere liberar un objeto que no fue creado por el Pool Manager: " + obj.name);
            return false;
        }

    }


    IObjectPool<Component> CreatePool(Component prefab, int defaultCapacity, int maxSize, bool createObjects, Transform parent)
    {
        int prefabID = prefab.gameObject.GetInstanceID();
        var pool = new ObjectPool<Component>(CreateFunc, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject,
            collectionCheck, defaultCapacity, maxSize);
        pools[prefabID] = pool;

        if (createObjects)
            CreateObjectsInPool(prefab, parent, pool, defaultCapacity);

        return pool;
    }

    void CreateObjectsInPool(Component prefab, Transform parent, IObjectPool<Component> pool, int defaultCapacity)
    {
        prefabTemp = prefab;
        parentTemp = parent;
        Component[] objectsCreated = new Component[defaultCapacity];
        for (int i = 0; i < objectsCreated.Length; i++)
            objectsCreated[i] = pool.Get();

        for (int i = 0; i < objectsCreated.Length; i++)
            pool.Release(objectsCreated[i]);
    }

    private Component CreateFunc()
    {
        var component = Instantiate(prefabTemp, parentTemp);
        int gameObjectID = component.gameObject.GetInstanceID();
        int prefabID = prefabTemp.gameObject.GetInstanceID();

        objectPoolLookup[gameObjectID] = pools[prefabID]; // Se asocia el pool al que pertenece el objeto recién creado
        componentLookup[gameObjectID] = component; // Se asocia la componente recién creada al object recién creado

        return component;
    }

    private void OnTakeFromPool(Component obj) => obj.gameObject.SetActive(true);
    
    private void OnReturnedToPool(Component obj) => obj.gameObject.SetActive(false);
    
    private void OnDestroyPoolObject(Component obj) => Destroy(obj.gameObject);
    

}
