using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Player.SkillTree;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SkillTreeManager : MonoBehaviour
{
    [SerializeField] private AssetReference ResetDataRef;
    private GameEvent resetDataEvent => EventLoader.Instance.GetEvent<GameEvent>(ResetDataRef);
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
