using _Project.Scripts.Core.Scriptable_Events;
using TheForgottenLetters;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AuthUtility : MonoBehaviour
{
    [Header("Events")] public AssetReference wantsToLoginOrSignUpRef;

    public AssetReference wantsToLogoutRef;

    private GameEvent loadedWantsToLoginOrSignUp => EventLoader.Instance.GetEvent<GameEvent>(wantsToLoginOrSignUpRef);
    private GameEvent loadedWantsToLogout => EventLoader.Instance.GetEvent<GameEvent>(wantsToLogoutRef);

    public void WantsToLoginOrSignUp()
    {
        loadedWantsToLoginOrSignUp.Raise();
    }

    public void Login()
    {
        loadedWantsToLoginOrSignUp.Raise();
    }

    public void Register()
    {
        loadedWantsToLoginOrSignUp.Raise();
    }

    public void Logout()
    {
        Auth.Instance.SignOutButton();
        loadedWantsToLogout.Raise();
    }
}