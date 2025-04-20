using Unity.Sentis;
using UnityEngine;

public abstract class HandwrittenClassifier : MonoBehaviour
{
    [ContextMenu("ExecuteModel")]
    public abstract void ExecuteModel(Texture2D tex);
}
