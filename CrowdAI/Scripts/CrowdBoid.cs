using UnityEngine;
using System.Collections.Generic;

namespace CrowdAI
{
    /// <summary>
    /// Основной компонент агента толпы. Управляет поведением отдельного персонажа.
    /// </summary>
    public class CrowdBoid : MonoBehaviour
    {
        [Header("Настройки движения")]
        [Tooltip("Максимальная скорость агента")]
        public float maxSpeed = 5f;
        
        [Tooltip("Ускорение агента")]
        public float acceleration = 2f;
        
        [Tooltip("Радиус восприятия соседей")]
        public float neighborRadius = 3f;
        
        [Header("Правила поведения (Boids)")]
        [Tooltip("Сила стремления держаться вместе")]
        public float cohesionWeight = 1f;
        
        [Tooltip("Сила избегания столкновений")]
        public float separationWeight = 2f;
        
        [Tooltip("Сила выравнивания направления с соседями")]
        public float alignmentWeight = 1.5f;
        
        [Tooltip("Минимальное расстояние для избегания столкновений")]
        public float minSeparationDistance = 1f;
        
        [Header("Цель (опционально)")]
        [Tooltip("Целевая точка движения")]
        public Transform target;
        
        [Tooltip("Сила притяжения к цели")]
        public float targetWeight = 2f;
        
        // Кэшированные компоненты
        private Rigidbody rb;
        private Vector3 velocity;
        
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
                rb.useGravity = false;
            }
            
            velocity = transform.forward * maxSpeed * 0.5f;
        }
        
        void Update()
        {
            CalculateBoidForces();
        }
        
        void CalculateBoidForces()
        {
            List<CrowdBoid> neighbors = GetNeighbors();
            
            Vector3 cohesion = CalculateCohesion(neighbors);
            Vector3 separation = CalculateSeparation(neighbors);
            Vector3 alignment = CalculateAlignment(neighbors);
            Vector3 targetForce = CalculateTargetForce();
            
            // Применяем веса к силам
            Vector3 totalForce = 
                cohesion * cohesionWeight +
                separation * separationWeight +
                alignment * alignmentWeight +
                targetForce * targetWeight;
            
            // Применяем силу к скорости
            velocity += totalForce * acceleration * Time.deltaTime;
            
            // Ограничиваем максимальную скорость
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
            
            // Применяем движение
            if (velocity.magnitude > 0.01f)
            {
                transform.position += velocity * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(velocity);
            }
        }
        
        List<CrowdBoid> GetNeighbors()
        {
            List<CrowdBoid> neighbors = new List<CrowdBoid>();
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, neighborRadius);
            
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject != gameObject)
                {
                    CrowdBoid boid = collider.GetComponent<CrowdBoid>();
                    if (boid != null)
                    {
                        neighbors.Add(boid);
                    }
                }
            }
            
            return neighbors;
        }
        
        Vector3 CalculateCohesion(List<CrowdBoid> neighbors)
        {
            if (neighbors.Count == 0) return Vector3.zero;
            
            Vector3 center = Vector3.zero;
            
            foreach (CrowdBoid neighbor in neighbors)
            {
                center += neighbor.transform.position;
            }
            
            center /= neighbors.Count;
            
            return (center - transform.position).normalized;
        }
        
        Vector3 CalculateSeparation(List<CrowdBoid> neighbors)
        {
            Vector3 separation = Vector3.zero;
            int count = 0;
            
            foreach (CrowdBoid neighbor in neighbors)
            {
                float distance = Vector3.Distance(transform.position, neighbor.transform.position);
                
                if (distance < minSeparationDistance && distance > 0)
                {
                    separation += (transform.position - neighbor.transform.position).normalized / distance;
                    count++;
                }
            }
            
            if (count > 0)
            {
                separation /= count;
            }
            
            return separation.normalized;
        }
        
        Vector3 CalculateAlignment(List<CrowdBoid> neighbors)
        {
            if (neighbors.Count == 0) return Vector3.zero;
            
            Vector3 averageVelocity = Vector3.zero;
            
            foreach (CrowdBoid neighbor in neighbors)
            {
                averageVelocity += neighbor.velocity;
            }
            
            averageVelocity /= neighbors.Count;
            
            return averageVelocity.normalized;
        }
        
        Vector3 CalculateTargetForce()
        {
            if (target == null) return Vector3.zero;
            
            return (target.position - transform.position).normalized;
        }
        
        // Отладка: рисуем радиус восприятия в редакторе
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, neighborRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minSeparationDistance);
        }
    }
}
