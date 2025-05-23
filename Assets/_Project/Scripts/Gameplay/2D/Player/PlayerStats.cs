using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.Player
{
    [CreateAssetMenu(menuName = "The Forgotten Letters/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")] public float acceleration = 10f;

        public float maxXSpeed = 5f;

        [Header("Jumping")] public float jumpSpeed = 5f;

        [Range(0f, 1f)] public float groundDecay = 0.9f;
        public float coyoteTimeDurationSec = 0.1f;

        [Header("Gravity")] public float groundGravity = 1f;

        public float jumpGravity = 1f;
        public float fallGravity = 1f;
    }
}