using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay._3D
{
    public class MovingPortal: MonoBehaviour
    {
        [SerializeField] float moveSpeed = 10f;
        [SerializeField] string linkedLetter;

        void Update()
        {
            // Move the portal toward the stationary player
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }

        public void SetLetter(string letter)
        {
            linkedLetter = letter;
            GetComponentInChildren<TextMeshPro>().text = letter;
        }
    }
}