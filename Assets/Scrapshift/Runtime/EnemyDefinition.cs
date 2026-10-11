using UnityEngine;

namespace Scrapshift
{
    [CreateAssetMenu(menuName = "Scrapshift/Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string title;
        public GameObject prefab;
        public float health = 20;
        public float speed = 1.5f;
        public float radius = .35f;
        public int scrap = 1;
        public Color color = Color.red;
    }
}
