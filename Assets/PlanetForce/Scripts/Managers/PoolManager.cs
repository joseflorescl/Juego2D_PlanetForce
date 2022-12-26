using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    /* Ej de uso del PoolManager:
     * - Crear un gameObject de nombre "PoolManager"
     * - Agregarle como componente este script
     * 
     * - Reemplazar la l�nea de c�digo: 
     *     EnemyController enemy = Instantiate(enemyPrefab, position, rotation);
     *   por:
     *     EnemyController enemy = PoolManager.Instance.Get(enemyPrefab, position, rotation);
     *     
     * - En este caso la var enemyPrefab est� declarada de tipo EnemyController. Para usar este PoolManager 
     *   solamente se necesita que el prefab sea de un tipo que herede de Component, 
         como cualquier script creado por nosotros que hereda de MonoBehaviour -> Behaviour -> Component

     * - Y en vez de hacer un Destroy del objeto, se reemplaza:
     *     Destroy(gameObject);
     *   por:
     *     PoolManager.Instance.Release(gameObject);
     *     
     * - NO es necesario realizar ninguna configuraci�n adicional para que el Pool Manager empiece a funcionar.
     *   Si se desea tener un control m�s detallado de cu�ntos objetos tendr� cada pool de cada prefab
     *   y si estos objetos se van a crear todos en el Start o se van a crear de a uno, se puede configurar 
     *   un nuevo elemento en el array poolData               
    */

    // Ser� singleton
    // Y tambi�n se configura que su orden de ejecuci�n sea primero que el resto de los scripts que hacen uso de "GameManager.Instance"
    // porque no estoy haciendo uso de lazy instantiation
    public static PoolManager instance = null;

    public static PoolManager Instance => instance;

    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 100;
    [SerializeField] private bool defaultCreateObjects = false;
    [SerializeField] private bool collectionCheck = true; // Recordar que esto solo es para el Editor, para ver errores en caso de usar mal el Release
    [SerializeField] private bool forceDestroy = true; //Indica si al ejecutar Release(obj) no existe pool configurado para ese obj, que hago: Destroy(obj) o nada.
    [SerializeField] private Transform defaultParent;
    [SerializeField] private int delayPoolCreation = 10; // Cada x elementos creados en el pool se descansar� 1 frame.

    [SerializeField] private PoolManagerData data;

    // Cada elemento de este dictionary es un pool para un prefab en particular.    
    // El int corresponde al prefab.gameObject.GetInstanceID()
    // En la funcion Get: necesito saber a que ObjectPool pertenece el prefab. La idea es tener una solucion O(1).
    // Por eso usamos un objeto Dictionary que me asocia el <id del prefab, ObjectPool>    
    Dictionary<int, IObjectPool<Component>> pools;

    // En la funcion Release: necesito saber cual es el ObjectPool al que pertenece el gameObject instanciado de un prefab
    // Por eso usamos un objeto Dictionary que me asocia el <id del gameObject, ObjectPool>    
    Dictionary<int, IObjectPool<Component>> objectPoolLookup;

    // Y adem�s en el Release necesitamos la Component que hay que liberar.
    //  Como al PoolManager lo llamar�n con un Release(gameObject), como no podemos hacer el GetComponent 
    //  de una componente que no sabemos cu�l es de todas las que tiene el gameObject,
    //  entonces se debe guardar esta relaci�n en otro dictionary:
    // Por eso usamos un objeto Dictionary que me asocia el <id del gameObject, Component creada con el Instantiate>    
    Dictionary<int, Component> componentLookup;

    // Para asociar cu�l es la data para crear un pool
    // Por eso usamos un objeto Dictionary que me asocia el <id del prefab, PoolData configurada en el Inspector>    
    Dictionary<int, PoolData> poolDataLookup;

    // En caso que se active el flag createParent, el Transform del objeto padre creado se podr� accesar con este dict
    //  que asocia el <id del prefab, Transform del padre de los objetos que se crean de ese prefab>
    Dictionary<int, Transform> parentLookup;

    // Como la funci�n CreateFunc no puede recibir argumentos, cuando se quiera hacer el Instantiate de un nuevo objeto
    // se usar�n estar vars
    Component prefabTemp;
    Transform parentTemp;

    private void Awake()
    {
        if (instance == null)
            instance = this;            
        else if (instance != this)
            Destroy(gameObject);

        pools = new Dictionary<int, IObjectPool<Component>>();
        objectPoolLookup = new Dictionary<int, IObjectPool<Component>>();
        componentLookup = new Dictionary<int, Component>();

        poolDataLookup = new Dictionary<int, PoolData>();
        parentLookup = new Dictionary<int, Transform>();

        FillPoolDataLookup();
    }

    void FillPoolDataLookup()
    {
        for (int i = 0; i < data.poolData.Length; i++)
        {
            var poolDataItem = data.poolData[i];
            int prefabID = poolDataItem.prefab.gameObject.GetInstanceID();
            poolDataLookup[prefabID] = poolDataItem;
        }
    }

    private void Start()
    {
        StartCoroutine(CreatePoolsModeStartRoutine()); 
    }

    IEnumerator CreatePoolsModeStartRoutine()
    {
        if (data)
        {
            for (int i = 0; i < data.poolData.Length; i++)
            {
                var poolDataItem = data.poolData[i];
                if (poolDataItem.createPoolMode == CreatePoolMode.Start)
                {
                    Transform parent = poolDataItem.createParent ? 
                        GetParentOrCreate(poolDataItem.prefab.gameObject.GetInstanceID()) : defaultParent;
                    yield return CreatePoolRoutine(poolDataItem.prefab, poolDataItem.defaultCapacity, poolDataItem.maxSize, true, parent);
                }
            }
        }
    }

    Transform GetParentOrCreate(int prefabID)
    {
        PoolData poolDataItem = poolDataLookup[prefabID];
        Transform parent;

        if (parentLookup.TryGetValue(prefabID, out parent))
            return parent;
        else
        {
            GameObject parentObject = new GameObject("Object Pool - " + poolDataItem.prefab.name);
            parent = parentObject.transform;
            parentLookup[prefabID] = parent;
            return parent;
        }
        
    }

    public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        var parent = GetParentOrCreate(prefab.gameObject.GetInstanceID());
        var obj = GetFromPool(prefab, parent);
        obj.transform.SetPositionAndRotation(position, rotation);
        return (T)obj;
    }

    public T Get<T>(T prefab, Transform parent) where T : Component
    {
        var obj = GetFromPool(prefab, parent);
        return (T)obj;
    }

    Component GetFromPool(Component prefab, Transform parent)
    {
        var pool = GetPoolOrCreate(prefab);
        // Importante: la funci�n pool.Get PUEDE llamar al m�todo CreateFunc, el cual har� el Instantiate
        //  por lo que se requiere tener seteado los valores de las vars prefabTemp y parentTemp, porque la
        //  funci�n CreateFunc NO puede recibir argumentos.
        prefabTemp = prefab;
        parentTemp = parent;
        var obj = pool.Get(); // Recordar que se setea var parentTemp en caso que funci�n CreateFunc haga un Instantiate
        obj.gameObject.SetActive(true);
        obj.transform.parent = parent; // Y si no se hizo el Instantiate, nos aseguramos setear correctamente el parent
        return obj;
    }

    //Transform GetParent(int prefabID)
    //{
    //    if (parentLookup.TryGetValue(prefabID, out var parent))
    //        return parent;
    //    else if (poolDataLookup[prefabID].createParent)
    //        // Condici�n de borde: si el objeto s� necesita un padre, pero todav�a no se ha creado, lo creamos ahora            
    //        return GetCreateParent(poolDataLookup[prefabID]);
    //    else
    //        return defaultParent;
    //}

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
                createObjects = poolDataItem.createPoolMode == CreatePoolMode.FirstGet
                    // Condici�n de borde: cuando se dispara una bala que tiene configurado su pool
                    // para ser creado en el Start, y todav�a la corutina se est� ejecutando y no se ha creado
                    // el pool de las balas todav�a: se agrega la condici�n del CreatePoolMode.Start
                    || poolDataItem.createPoolMode == CreatePoolMode.Start 
                    || defaultCreateObjects;
                parent = poolDataItem.createParent ? GetParentOrCreate(prefabID) : defaultParent;
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

    public bool Release(GameObject obj)
    {
        // Condici�n de borde: si se crea un objeto extra al maxSize, entonces ese objeto ser� destruido con Destroy,
        //  no lo mantendremos en el pool.
        //  Pero si ese objeto "muere" 2 veces en el mismo frame, por ej 2 balas lo chocan a la vez, entonces se llamar�
        //  2 veces este m�todo Release: la primera vez el objeto ser� destruido, y la segunda vez el m�todo se cae
        //  porque la var obj ya es null. Por eso se debe validar el null:
        if (!obj)
            return false; // Un objeto ya fue destruido del pool. Se est� tratando de hacer un Release

        // Para validar si se est� tratando de hacer Release 2 veces de un objeto ya devuelto al pool:
        //  una validaci�n simple es si el obj ya est� desactivado:
        if (collectionCheck && !obj.activeInHierarchy)
            return false; // Release de un objeto ya desactivado

        var gameObjectID = obj.GetInstanceID();
        if (objectPoolLookup.TryGetValue(gameObjectID, out var pool))
        {
            var component = componentLookup[gameObjectID]; // Buscamos la componente a liberar
            pool.Release(component);
            return true;
        }
        else
        {
            if (forceDestroy)
                Destroy(obj);
            return false; // Se quiere liberar un objeto que no fue creado por el Pool Manager
        }

    }


    IObjectPool<Component> CreatePool(Component prefab, int defaultCapacity, int maxSize, bool createObjects, Transform parent)
    {
        int prefabID = prefab.gameObject.GetInstanceID();
        IObjectPool<Component> pool;
        
        if (pools.TryGetValue(prefabID, out pool)) // Primero se valida si es que el pool ya existe
            return pool;
        
        pool = new ObjectPool<Component>(CreateFunc, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject,
            collectionCheck, defaultCapacity, maxSize);
        pools[prefabID] = pool;

        prefab.gameObject.SetActive(false); //Todos los clones quedaran en estado desactivado

        if (createObjects)
            CreateObjectsInPool(prefab, parent, pool, defaultCapacity);

        return pool;
    }

    IEnumerator CreatePoolRoutine(Component prefab, int defaultCapacity, int maxSize, bool createObjects, Transform parent)
    {
        int prefabID = prefab.gameObject.GetInstanceID();
        IObjectPool<Component> pool;

        if (pools.TryGetValue(prefabID, out pool)) // Primero se valida si es que el pool ya existe
            yield break;

        // Nos aseguramos de SOLO crear el pool, sin ning�n gameObject
        pool = CreatePool(prefab, defaultCapacity, maxSize, false, parent);

        // Ahora validamos si hay que crear los objetos: se crear�n usando una corutina
        if (createObjects)
            //yield return StartCoroutine(CreateObjectsInPoolRoutine(prefab, parent, pool, defaultCapacity));
            yield return CreateObjectsInPoolRoutine(prefab, parent, pool, defaultCapacity);
    }



    void CreateObjectsInPool(Component prefab, Transform parent, IObjectPool<Component> pool, int defaultCapacity)
    {
        // Notar que si los objetos del Pool se crean en runtime (no en el Start), entonces si son muchos objetos
        //  se va a notar una peque�a baja en los FPS.
        // Yo solo estoy usando FirstGet para los Sprites Renderers del Background.
        prefabTemp = prefab;
        parentTemp = parent;
        Component[] objectsCreated = new Component[defaultCapacity];
        for (int i = 0; i < objectsCreated.Length; i++)
            objectsCreated[i] = pool.Get();

        for (int i = 0; i < objectsCreated.Length; i++)
            pool.Release(objectsCreated[i]);
    }

    IEnumerator CreateObjectsInPoolRoutine(Component prefab, Transform parent, IObjectPool<Component> pool, int defaultCapacity)
    {
        Component[] objectsCreated = new Component[defaultCapacity];
        for (int i = 0; i < objectsCreated.Length; i++)
        {
            prefabTemp = prefab;
            parentTemp = parent;
            objectsCreated[i] = pool.Get(); // Este Get llamar� a un Instantiate

            if (i % delayPoolCreation == 0)
                yield return null;
        }

        for (int i = 0; i < objectsCreated.Length; i++)
        {
            pool.Release(objectsCreated[i]);
        }
    }

    private Component CreateFunc()
    {
        var component = Instantiate(prefabTemp, parentTemp);
        int gameObjectID = component.gameObject.GetInstanceID();
        int prefabID = prefabTemp.gameObject.GetInstanceID();

        objectPoolLookup[gameObjectID] = pools[prefabID]; // Se asocia el pool al que pertenece el objeto reci�n creado
        componentLookup[gameObjectID] = component; // Se asocia la componente reci�n creada al object reci�n creado

        return component;
    }

    private void OnTakeFromPool(Component obj) { } // El SetActive(true) se hace en el m�todo Get de Pool Manager

    private void OnReturnedToPool(Component obj) => obj.gameObject.SetActive(false);

    private void OnDestroyPoolObject(Component obj)
    {
        print("PoolManager: Destroy objeto que excede el maxSize: " + obj.name);
        Destroy(obj.gameObject);
    }
}
