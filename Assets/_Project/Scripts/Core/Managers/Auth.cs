using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TheForgottenLetters
{
    public class Auth : MonoBehaviour
    {
        [SerializeField] private GameObject authPanel;
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private TMP_Text statusText;

        [SerializeField] private double delayBeforeContinue = 2f; // Delay in seconds


        [Header("Events")] [SerializeField] private AssetReference OnLoginOrSignUp;

        private FirebaseAuth auth;
        private FirebaseFirestore db;
        private TimeSpan DelayBeforeContinue => TimeSpan.FromSeconds(delayBeforeContinue);

        public static Auth Instance { get; private set; }
        public bool IsLoggedIn => auth != null && auth.CurrentUser != null;
        public string UserId => auth != null && auth.CurrentUser != null ? auth.CurrentUser.UserId : null;
        public string UserEmail => auth != null && auth.CurrentUser != null ? auth.CurrentUser.Email : null;
        private GameEvent onLoginOrSignUp => EventLoader.Instance.GetEvent<GameEvent>(OnLoginOrSignUp);

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
        }

        private void OnDestroy()
        {
            FirebaseManager.Instance.OnFirebaseInitialized -= BeginAuth;
        }

        private void BeginAuth()
        {
            ActivateAuthScreen();

            UpdateStatus("Initializing Firebase...");
            if (FirebaseManager.Instance == null)
            {
                Debug.LogError("FirebaseManager is not initialized.");
                UpdateStatus("Firebase is not ready.");
            }

            if (FirebaseManager.Instance != null && FirebaseManager.Instance.Auth != null)
            {
                auth = FirebaseManager.Instance.Auth;
                db = FirebaseManager.Instance.Firestore;
            }
            else
            {
                Debug.LogError("FirebaseManager or Firebase Auth is not initialized.");
                UpdateStatus("Firebase is not ready.");
            }

            if (auth.CurrentUser != null)
            {
                UpdateStatus("User is already signed in: " + auth.CurrentUser.Email);
                FirebaseManager.Instance.StartListeningForChanges();
                SaveAuthInfo();
                Continue();
            }
            else
            {
                UpdateStatus("No user is currently signed in.");
            }
        }

        public void DeactivateAuthScreen()
        {
            authPanel.SetActive(false);
        }

        public void ActivateAuthScreen()
        {
            authPanel.SetActive(true);
        }

        private void UpdateStatus(string message)
        {
            statusText.text = message;
            Debug.Log(message);
        }

        [ContextMenu("SignUpButton")]
        public void SignUpButton()
        {
            SignUp().Forget();
        }

        public void LoginButton()
        {
            Login().Forget();
        }

        public void SignOutButton()
        {
            SignOut().Forget();
        }

        public async UniTaskVoid SignUp()
        {
            var email = emailInputField.text;
            var password = passwordInputField.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                UpdateStatus("Email and password must not be empty.");
                return;
            }

            try
            {
                await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                UpdateStatus("Sign Up Successful!");
                FirebaseManager.Instance.StartListeningForChanges();
                SaveAuthInfo();
                await ContinueAfterDelay();
                onLoginOrSignUp.Raise();
            }
            catch (Exception ex)
            {
                UpdateStatus("Sign Up Failed: " + ex.Message);
            }
        }

        public async UniTaskVoid Login()
        {
            var email = emailInputField.text;
            var password = passwordInputField.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                UpdateStatus("Email and password must not be empty.");
                return;
            }

            try
            {
                await auth.SignInWithEmailAndPasswordAsync(email, password);
                UpdateStatus("Login Successful!");
                FirebaseManager.Instance.StartListeningForChanges();
                SaveAuthInfo();
                await ContinueAfterDelay();
                onLoginOrSignUp.Raise();
            }
            catch (Exception ex)
            {
                UpdateStatus("Login Failed: " + ex.Message);
            }
        }

        public async UniTaskVoid SignOut()
        {
            if (auth.CurrentUser == null)
            {
                UpdateStatus("No user is currently signed in.");
                return;
            }

            try
            {
                auth.SignOut();
                UpdateStatus("Sign Out Successful!");
                FirebaseManager.Instance.StopListeningForChanges();
                await ContinueAfterDelay();
            }
            catch (Exception ex)
            {
                UpdateStatus("Sign Out Failed: " + ex.Message);
            }
        }

        public void SaveAuthInfo()
        {
            if (auth.CurrentUser == null)
            {
                UpdateStatus("Error: Must be logged in to save data.");
                return;
            }

            if (db == null)
            {
                UpdateStatus("Error: Firestore not initialized.");
                return;
            }

            var userId = auth.CurrentUser.UserId;

            var userData = new Dictionary<string, object>
            {
                { "userId", userId },
                { "email", auth.CurrentUser.Email }
            };

            UpdateStatus("Saving Data...");
            db.Collection("users").Document(userId)
                .SetAsync(userData, SetOptions.MergeAll)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        UpdateStatus("Data saved successfully!");
                    else if (task.IsFaulted)
                        UpdateStatus("Error saving Data: " + task.Exception);
                    else if (task.IsCanceled)
                        UpdateStatus("Saving Data was cancelled.");
                });
        }

        private async UniTask ContinueAfterDelay()
        {
            await UniTask.Delay(DelayBeforeContinue);
            Continue();
        }

        private void Continue()
        {
            UpdateStatus("Ready to proceed...");
            DeactivateAuthScreen();
        }
    }
}