using UnityEngine;

namespace CrowdAI
{
    /// <summary>
    /// Избегание препятствий для агентов толпы.
    /// </summary>
    public class CrowdObstacleAvoidance : MonoBehaviour
    {
        [Header("Настройки избегания")]
        [Tooltip("Дистанция обнаружения препятствий")]
        public float obstacleDetectionRadius = 2f;
        
        [Tooltip("Сила избегания препятствий")]
        public float avoidanceWeight = 3f;
        
        [Tooltip("Маска слоев для препятствий")]
        public LayerMask obstacleLayers = -1;
        
        [Tooltip("Игнорировать других агентов (они обрабатываются отдельно)")]
        public bool ignoreAgents = true;
        
        private CrowdBoid boid;
        
        void Start()
        {
            boid = GetComponent<CrowdBoid>();
            
            if (boid == null)
            {
                Debug.LogError("CrowdObstacleAvoidance требует компонент CrowdBoid!");
                enabled = false;
            }
        }
        
        // Вызывается из CrowdBoid через SendMessage или напрямую
        public Vector3 CalculateObstacleAvoidance(Vector3 velocity)
        {
            Vector3 avoidanceForce = Vector3.zero;
            
            Collider[] obstacles = Physics.OverlapSphere(
                transform.position, 
                obstacleDetectionRadius, 
                obstacleLayers
            );
            
            foreach (Collider obstacle in obstacles)
            {
                // Игнорируем других агентов если настроено
                if (ignoreAgents && obstacle.GetComponent<CrowdBoid>() != null)
                {
                    continue;
                }
                
                Vector3 toObstacle = transform.position - obstacle.ClosestPoint(transform.position);
                float distance = toObstacle.magnitude;
                
                if (distance > 0 && distance < obstacleDetectionRadius)
                {
                    // Чем ближе препятствие, тем сильнее отталкивание
                    float strength = (obstacleDetectionRadius - distance) / obstacleDetectionRadius;
                    avoidanceForce += toObstacle.normalized * strength;
                }
            }
            
            return avoidanceForce.normalized * avoidanceWeight;
        }
        
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, obstacleDetectionRadius);
        }
    }
}
