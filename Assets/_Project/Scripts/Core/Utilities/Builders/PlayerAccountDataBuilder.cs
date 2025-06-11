using _Project.Scripts.Core.Managers;
using Cysharp.Threading.Tasks;
using TheForgottenLetters;

public class PlayerAccountDataBuilder
{
    public string fullName { get; set; }

    public string username { get; set; }

    public string email { get; set; }

    public bool hasDyslexiaTest { get; set; }

    public PlayerAccountData SoRef => PersistentSOManager.GetSO<PlayerAccountData>();


    public PlayerAccountDataBuilder()
    {
        fullName = string.Empty;
        username = string.Empty;
        email = string.Empty;
        hasDyslexiaTest = false;
    }

    public PlayerAccountDataBuilder(PlayerAccountData existingData)
    {
        fullName = existingData.fullName;
        username = existingData.username;
        email = existingData.email;
        hasDyslexiaTest = existingData.hasDyslexiaTest;
    }

    public PlayerAccountDataBuilder SetFullName(string fullName)
    {
        this.fullName = fullName;
        return this;
    }

    public PlayerAccountDataBuilder SetUsername(string username)
    {
        this.username = username;
        return this;
    }

    public PlayerAccountDataBuilder SetEmail(string email)
    {
        this.email = email;
        return this;
    }

    public PlayerAccountDataBuilder UpdateLocalData()
    {
        SoRef.UpdateLocalData(this);
        return this;
    }

    public PlayerAccountDataSerializable SerializePlayerAccountData()
    {
        return new PlayerAccountDataSerializable(SoRef);
    }

    public async UniTask SaveDataToFirebaseAsync()
    {
        await Auth.Instance.SaveAuthInfoAsync(fullName, username, email);
    }
}