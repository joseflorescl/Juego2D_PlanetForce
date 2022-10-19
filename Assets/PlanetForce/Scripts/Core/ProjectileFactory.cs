using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFactory : MonoBehaviour
{
    [System.Serializable]
    public struct FirePoint
    {
        public Transform transform;
        public KinematicProjectile projectilePrefab; // Debe tener una componente que implemente la interfaz IKinematicProjectile        
    }

    public enum CreationMethod { AllFirePointsAtOnce, OneRandomFirePoint}

    [SerializeField] private CreationMethod creationMethod = CreationMethod.AllFirePointsAtOnce;
    [SerializeField] private FirePoint[] firePoints;
    [SerializeField] private bool rotateToVelocityDirection = false;

    List<FirePoint> activeFirePoints;
    float explosionDamageRange = 0.2f;

    int min;
    int max;

    private void Awake()
    {
        activeFirePoints = new List<FirePoint>(firePoints);
    }


    public bool Create()
    {
        return CreateOptionalTarget(null);

    }

    public bool CreateToTarget(Vector3 targetPosition)
    {
        return CreateOptionalTarget(targetPosition);
    }

    bool CreateOptionalTarget(Vector3? targetPosition)
    {
        if (activeFirePoints.Count == 0) return false;

        CalculateMinMaxRange();

        for (int i = min; i < max; i++)
        {
            var position = activeFirePoints[i].transform.position;
            var direction = targetPosition != null ? targetPosition.Value - position : activeFirePoints[i].transform.up;
            InstantiateProjectile(activeFirePoints[i].projectilePrefab, position, direction);
        }

        return true;
    }


    void InstantiateProjectile(KinematicProjectile projectilePrefab, Vector3 position, Vector3 velocityDirection)
    {
        // Esto es así para para implementar el LookAt en 2D, en donde el eje forward es en realidad el eje Y
        Quaternion rotation;
        if (rotateToVelocityDirection)
            rotation = Quaternion.LookRotation(Vector3.forward, velocityDirection);
        else
            rotation = projectilePrefab.transform.rotation;
        
        var projectile = Instantiate(projectilePrefab, position, rotation);
        // Notar que si se usa una interfaz IKinematicProjectile estamos obligados a usar GetComponent (performance!),
        //  go.GetComponent<IKinematicProjectile>()
        // porque el argumento projectilePrefab
        //  no puede declararse de tipo IKinematicProjectile, debe ser de una clase concreta que herede de MB.
        // Por eso se usa la opción de la herencia, declarando una clase padre KinematicProjectile de la cual
        // heredan las Bullet y las Entity
        projectile.SetKinematicVelocity(velocityDirection, projectile.Speed);
    }

    void CalculateMinMaxRange()
    {                
        switch (creationMethod)
        {
            case CreationMethod.AllFirePointsAtOnce:
                min = 0;
                max = activeFirePoints.Count;
                break;
            case CreationMethod.OneRandomFirePoint:
                min = Random.Range(0, activeFirePoints.Count);
                max = min + 1;
                break;
        }
    }


    public void AnalyzeExplosion(Vector3 explosionPosition)
    {
        // Si un firePoint esta muy cerca del explosionPosition, entonces ese firePoint se elimina: la nave ya no dispará por ahí
        for (int i = activeFirePoints.Count - 1; i >= 0; i--)
        {
            if (Vector2.Distance(activeFirePoints[i].transform.position, explosionPosition) <= explosionDamageRange)
            {
                activeFirePoints.RemoveAt(i);
            }
        }
    }

}
