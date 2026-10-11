using UnityEngine;

namespace Scrapshift
{
    public enum WeaponKind { Rivet, Orbit, Pulse }
    [CreateAssetMenu(menuName = "Scrapshift/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string title;
        [TextArea] public string description;
        public WeaponKind kind;
        public float damage = 10;
        public float interval = .5f;
        public float range = 8;
        public GameObject effectPrefab;
        public Material accent;
    }
}
