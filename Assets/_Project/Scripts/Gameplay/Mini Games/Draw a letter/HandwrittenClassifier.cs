using UnityEngine;

namespace _Project.Scripts.Gameplay.Mini_Games.Draw_a_letter
{
    public abstract class HandwrittenClassifier : MonoBehaviour
    {
        [ContextMenu("ExecuteModel")]
        public abstract void ExecuteModel(Texture2D tex);
    }
}