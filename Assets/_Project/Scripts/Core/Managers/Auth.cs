using System;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.SaveSystem;
using _Project.Scripts.Core.Scriptable_Events;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TheForgottenLetters
{
    public class Auth : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private AuthUIManager authUIManager;

        [Header("Configuration")]
        [SerializeField] private double delayBeforeContinue = 2f;

        [Header("Events")]
        [SerializeField] private AssetReference OnLoginOrSignUp;

        private FirebaseAuth auth;
        private FirebaseFirestore db;
        private ListenerRegistration accountListener;

        private TimeSpan DelayBeforeContinue => TimeSpan.FromSeconds(delayBeforeContinue);
        private GameEvent onLoginOrSignUp => EventLoader.Instance.GetEvent<GameEvent>(OnLoginOrSignUp);

        public static Auth Instance { get; private set; }
        public bool IsLoggedIn => auth?.CurrentUser != null;
        public string UserId => auth?.CurrentUser?.UserId;
        public string UserEmail => auth?.CurrentUser?.Email;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            FirebaseManager.Instance.OnFirebaseInitialized += BeginAuth;
            if (FirebaseManager.Instance.IsInitialized)
                BeginAuth();
        }

        private void OnDestroy()
        {
            if (FirebaseManager.Instance != null)
                FirebaseManager.Instance.OnFirebaseInitialized -= BeginAuth;
            authUIManager?.HideAuthRoot();
        }

        private void BeginAuth()
        {
            // UpdateStatus("Initializing Auth...");

            auth = FirebaseManager.Instance.Auth;
            db = FirebaseManager.Instance.Firestore;

            if (auth == null)
            {
                Debug.LogError("Firebase Auth is not initialized.");
                authUIManager.ShowStatusMessage("Firebase is not ready.");
                return;
            }

            if (auth.CurrentUser != null)
            {
                // UpdateStatus($"User already signed in: {UserEmail}");
                FirebaseManager.Instance.StartListeningForChanges();
                Continue();
            }
            else
            {
                // UpdateStatus("No user is currently signed in.");
            }
        }

        public void SignUpButton() => SignUp().Forget();
        public void LoginButton() => Login().Forget();
        public void SignOutButton() => SignOut().Forget();

        public void ActivateAuthScreen() => authUIManager.ShowAuthRoot();
        public void DeactivateAuthScreen() => authUIManager.HideAuthRoot();

        private bool IsInputValid(string email, string password, string confirmPassword = null)
        {
            return !string.IsNullOrEmpty(email) &&
                   email.Contains("@") &&
                   !string.IsNullOrEmpty(password) &&
                   password.Length >= 6;
        }

        public async UniTask SignUp()
        {
            var fullName = authUIManager.regFullNameInput.text;
            var username = authUIManager.regUsernameInput.text;
            var email = authUIManager.regEmailInput.text;
            var password = authUIManager.regPasswordInput.text;
            var confirmPassword = authUIManager.regConfirmPasswordInput.text;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
            {
                authUIManager.ShowStatusMessage("Full name and username are required.");
                return;
            }
            if (password != confirmPassword)
            {
                authUIManager.ShowStatusMessage("Passwords do not match.");
                return;
            }

            if (!IsInputValid(email, password))
            {
                authUIManager.ShowStatusMessage("Invalid email or weak password.");
                return;
            }

            try
            {
                await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                // UpdateStatus("Sign Up Successful!");
                FirebaseManager.Instance.StartListeningForChanges();
                await SaveAuthInfoAsync(fullName, username, email);
                ListenToProfileChanges();
                await ContinueAfterDelay();
                onLoginOrSignUp.Raise();
            }
            catch (FirebaseException ex)
            {
                HandleFirebaseError(ex, "Sign Up Failed");
            }
        }

        public async UniTask Login()
        {
            var email = authUIManager.loginEmailInput.text;
            var password = authUIManager.loginPasswordInput.text;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                authUIManager.ShowStatusMessage("Email and password are required.");
                return;
            }

            if (!IsInputValid(email, password))
            {
                authUIManager.ShowStatusMessage("Invalid email or password.");
                return;
            }


            try
            {
                await auth.SignInWithEmailAndPasswordAsync(email, password);
                // UpdateStatus("Login Successful!");
                FirebaseManager.Instance.StartListeningForChanges();
                ListenToProfileChanges();
                await ContinueAfterDelay();
                onLoginOrSignUp.Raise();
            }
            catch (FirebaseException ex)
            {
                HandleFirebaseError(ex, "Login Failed");
            }
        }

        public async UniTask SignOut()
        {
            if (!IsLoggedIn)
            {
                authUIManager.ShowStatusMessage("No user is currently signed in.");
                return;
            }

            try
            {
                auth.SignOut();
                ResetAllData();
                FirebaseManager.Instance.StopListeningForChanges();
                StopListenToProfileChanges();
                // UpdateStatus("Sign Out Successful!");
                await ContinueAfterDelay();
            }
            catch (Exception ex)
            {
                authUIManager.ShowStatusMessage("Sign Out Failed: " + ex.Message);
            }
        }

        private void ResetAllData()
        {
            PersistentSOManager.GetSO<PlayerAccountData>().ResetData();
            PersistentSOManager.GetSO<PlayerAbilityStats>().ResetData();
            PersistentSOManager.GetSO<DrawLetterData>().ResetData();
            PersistentSOManager.GetSO<ObjectDetectionData>().ResetData();
            PersistentSOManager.GetSO<LetterHuntData>().ResetData();
        }

        public async UniTask SaveAuthInfoAsync(string fullName = null, string username = null, string email = null)
        {
            if (!IsLoggedIn || db == null)
            {
                authUIManager.ShowStatusMessage("Error: Must be logged in and have Firestore initialized to save data.");
                return;
            }

            var userData = PersistentSOManager.GetSO<PlayerAccountData>()
                                              .GetBuilder()
                                              .SetEmail(email)
                                              .SetFullName(fullName)
                                              .SetUsername(username)
                                              .UpdateLocalData()
                                              .SerializePlayerAccountData();

            // UpdateStatus("Saving Data...");

            try
            {
                await db.Collection("users").Document(UserId).SetAsync(userData, SetOptions.MergeAll);
                // UpdateStatus("Data saved successfully!");
            }
            catch (OperationCanceledException)
            {
                authUIManager.ShowStatusMessage("Saving Data was cancelled.");
            }
            catch (Exception ex)
            {
                authUIManager.ShowStatusMessage("Error saving Data: " + ex.Message);
            }
        }

        private void ListenToProfileChanges()
        {
            if (!IsLoggedIn || db == null)
            {
                authUIManager.ShowStatusMessage("Error: Must be logged in and have Firestore initialized to listen for changes.");
                return;
            }

            var docRef = db.Collection("users").Document(UserId);
            accountListener = docRef.Listen(snapshot =>
            {
                try
                {
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ConvertTo<PlayerAccountDataSerializable>();
                        Debug.Log($"[REAL-TIME] Name: {data.fullName}, userName: {data.username}, email: {data.email}, hasDyslexiaTest: {data.hasDyslexiaTest}");
                        PersistentSOManager.GetSO<PlayerAccountData>()?.UpdateLocalData(data);
                    }
                    else
                    {
                        Debug.LogWarning("No Account found.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Firestore Listen Error (Player Account): " + e.Message);
                }
            });
        }

        private void StopListenToProfileChanges()
        {
            accountListener?.Stop();
        }

        private async UniTask ContinueAfterDelay()
        {
            await UniTask.Delay(DelayBeforeContinue);
            Continue();
        }

        private void Continue()
        {
            // UpdateStatus("Ready to proceed...");
            authUIManager.HideAuthRoot();
        }

        private void HandleFirebaseError(FirebaseException ex, string context)
        {
            var message = ex.Message;
            switch ((AuthError)ex.ErrorCode)
            {
                case AuthError.EmailAlreadyInUse:
                    message = "Email already in use.";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid email format.";
                    break;
                case AuthError.WrongPassword:
                    message = "Incorrect password.";
                    break;
                case AuthError.UserNotFound:
                    message = "User not found.";
                    break;
            }
            authUIManager.ShowStatusMessage($"{context}: {message}");
        }
    }
}
