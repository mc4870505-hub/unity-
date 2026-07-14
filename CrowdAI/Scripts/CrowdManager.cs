using UnityEngine;
using System.Collections.Generic;

namespace CrowdAI
{
    /// <summary>
    /// Менеджер толпы. Создает и управляет группой агентов.
    /// </summary>
    public class CrowdManager : MonoBehaviour
    {
        [Header("Настройки спавна")]
        [Tooltip("Префаб агента толпы")]
        public GameObject agentPrefab;
        
        [Tooltip("Количество агентов в толпе")]
        [Range(10, 500)]
        public int agentCount = 50;
        
        [Tooltip("Размер области спавна (куб)")]
        public float spawnAreaSize = 20f;
        
        [Header("Общая цель")]
        [Tooltip("Целевая точка для всей толпы")]
        public Transform globalTarget;
        
        [Tooltip("Показывать ли gizmos для отладки")]
        public bool showGizmos = true;
        
        // Список всех созданных агентов
        private List<CrowdBoid> agents = new List<CrowdBoid>();
        
        void Start()
        {
            SpawnAgents();
        }
        
        void SpawnAgents()
        {
            // Очищаем предыдущих агентов если они есть
            foreach (CrowdBoid agent in agents)
            {
                if (agent != null)
                {
                    Destroy(agent.gameObject);
                }
            }
            
            agents.Clear();
            
            // Создаем новых агентов
            for (int i = 0; i < agentCount; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                
                GameObject agentObj = Instantiate(agentPrefab, spawnPos, Quaternion.identity, transform);
                CrowdBoid agent = agentObj.GetComponent<CrowdBoid>();
                
                if (agent == null)
                {
                    agent = agentObj.AddComponent<CrowdBoid>();
                }
                
                // Устанавливаем общую цель
                if (globalTarget != null)
                {
                    agent.target = globalTarget;
                }
                
                agents.Add(agent);
            }
            
            Debug.Log($"Создано {agents.Count} агентов толпы");
        }
        
        Vector3 GetRandomSpawnPosition()
        {
            float randomX = Random.Range(-spawnAreaSize / 2f, spawnAreaSize / 2f);
            float randomY = Random.Range(-spawnAreaSize / 2f, spawnAreaSize / 2f);
            float randomZ = Random.Range(-spawnAreaSize / 2f, spawnAreaSize / 2f);
            
            return transform.position + new Vector3(randomX, randomY, randomZ);
        }
        
        // Публичный метод для добавления агента во время выполнения
        public void AddAgent(Vector3 position)
        {
            if (agentPrefab == null)
            {
                Debug.LogWarning("Agent prefab не назначен!");
                return;
            }
            
            GameObject agentObj = Instantiate(agentPrefab, position, Quaternion.identity, transform);
            CrowdBoid agent = agentObj.GetComponent<CrowdBoid>();
            
            if (agent == null)
            {
                agent = agentObj.AddComponent<CrowdBoid>();
            }
            
            if (globalTarget != null)
            {
                agent.target = globalTarget;
            }
            
            agents.Add(agent);
        }
        
        // Публичный метод для удаления случайного агента
        public void RemoveRandomAgent()
        {
            if (agents.Count > 0)
            {
                int randomIndex = Random.Range(0, agents.Count);
                CrowdBoid agentToRemove = agents[randomIndex];
                
                agents.RemoveAt(randomIndex);
                
                if (agentToRemove != null)
                {
                    Destroy(agentToRemove.gameObject);
                }
            }
        }
        
        // Изменить количество агентов во время выполнения
        public void SetAgentCount(int newCount)
        {
            agentCount = Mathf.Max(0, newCount);
            
            if (agents.Count < agentCount)
            {
                // Добавить агентов
                int toAdd = agentCount - agents.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    AddAgent(GetRandomSpawnPosition());
                }
            }
            else if (agents.Count > agentCount)
            {
                // Удалить лишних агентов
                int toRemove = agents.Count - agentCount;
                for (int i = 0; i < toRemove; i++)
                {
                    RemoveRandomAgent();
                }
            }
        }
        
        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            
            // Рисуем область спавна
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * spawnAreaSize);
            
            // Рисуем цель
            if (globalTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(globalTarget.position, 1f);
            }
        }
    }
}
