using UnityEngine;

namespace _Project.Scripts.Gameplay._2D
{
    [CreateAssetMenu(menuName = "The Forgotten Letters/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        public float jumpSpeed = 5;
        public float gravityForce = 1;
        public float acceleration;
        public float maxXSpeed = 5;
        [Range(0f,1f)] public float groundDecay = .9f;
    }
}
