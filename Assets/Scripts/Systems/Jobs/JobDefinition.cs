using UnityEngine;

namespace KanairoCity.Systems.Jobs
{
    [CreateAssetMenu(fileName = "NewJobDefinition", menuName = "KanairoCity/Jobs/JobDefinition")]
    public class JobDefinition : ScriptableObject
    {
        public string jobName;
        [TextArea] public string description;
        public Sprite jobIcon;
        public float baseSalary = 100f;
        
        [Header("Requirements")]
        public float minReputation = 0f;
        public GameObject workspacePrefab;
        
        [Header("NPC Interaction")]
        public string[] customerRequestLines;
        public string[] successLines;
        public string[] failureLines;
    }
}
