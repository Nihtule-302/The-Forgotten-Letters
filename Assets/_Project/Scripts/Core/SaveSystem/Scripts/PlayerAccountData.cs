
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAccountData", menuName = "Player/PlayerAccountData")]
public class PlayerAccountData : ScriptableObject
{
    public string fullName;
    public string username;
    public string email;
    public bool hasDyslexiaTest;

    [ContextMenu("Reset and Save Data To Firebase")]
    public void ResetAndSaveDataToFirebase()
    {
        ResetData();
        SaveData();
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        fullName = string.Empty;
        username = string.Empty;
        email = string.Empty;
        hasDyslexiaTest = false;
    }

    [ContextMenu("Save Data")]
    private void SaveData()
    {
        SaveDataAsync().Forget();
    }

    // ========== Async Save ==========
    private async UniTask SaveDataAsync()
    {
        var builder = GetBuilder();
        if (builder == null)
        {
            Debug.LogError("PlayerAccountDataBuilder is null.");
            return;
        }
        await builder.SaveDataToFirebaseAsync();
    }

    public void UpdateLocalData(PlayerAccountDataBuilder builder)
    {
        fullName = builder.fullName;
        username = builder.username;
        email = builder.email;
        hasDyslexiaTest = builder.hasDyslexiaTest;
    }

    public void UpdateLocalData(PlayerAccountDataSerializable playerAccountData)
    {
        fullName = playerAccountData.fullName;
        username = playerAccountData.username;
        email = playerAccountData.email;
        hasDyslexiaTest = playerAccountData.hasDyslexiaTest;
    }

    public PlayerAccountDataBuilder GetBuilder()
    {
        return new PlayerAccountDataBuilder(this);
    }
}

[FirestoreData]
public class PlayerAccountDataSerializable
{
    [FirestoreProperty] public string fullName { get; set; }

    [FirestoreProperty] public string username { get; set; }

    [FirestoreProperty] public string email { get; set; }

    [FirestoreProperty] public bool hasDyslexiaTest { get; set; }

    public PlayerAccountDataSerializable()
    {
        fullName = string.Empty;
        username = string.Empty;
        email = string.Empty;
        hasDyslexiaTest = false;
    }

    public PlayerAccountDataSerializable(PlayerAccountData data)
    {
        fullName = data.fullName;
        username =  data.username;
        email =  data.email;
        hasDyslexiaTest = data.hasDyslexiaTest;
    }

    
}
