using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TheForgottenLetters
{
    public class Auth : MonoBehaviour
    {
        [SerializeField] private GameObject authPanel;
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private TMP_Text statusText;

        [SerializeField] private double delayBeforeContinue = 2f; // Delay in seconds
        private TimeSpan DelayBeforeContinue => TimeSpan.FromSeconds(delayBeforeContinue);

        private FirebaseAuth auth;
        private FirebaseFirestore db;


        void Start()
        {
            FirebaseManager.Instance.OnFirebaseInitialized += BeginAuth;
        }
        void OnDestroy()
        {
            FirebaseManager.Instance.OnFirebaseInitialized -= BeginAuth;
        }

        private void BeginAuth()
        {
            authPanel.SetActive(true);

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

        void UpdateStatus(string message)
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

        public async UniTaskVoid SignUp()
        {
            string email = emailInputField.text;
            string password = passwordInputField.text;

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
            }
            catch (Exception ex)
            {
                UpdateStatus("Sign Up Failed: " + ex.Message);
            }
        }

        public async UniTaskVoid Login()
        {
            string email = emailInputField.text;
            string password = passwordInputField.text;

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
            }
            catch (Exception ex)
            {
                UpdateStatus("Login Failed: " + ex.Message);
            }
        }

        public void SignOut()
        {
            if (auth.CurrentUser != null)
            {
                auth.SignOut();
                UpdateStatus("Signed Out");
            }
            else
            {
                UpdateStatus("No user is currently signed in.");
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

            string userId = auth.CurrentUser.UserId;

            Dictionary<string, object> userData = new Dictionary<string, object>
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
            authPanel.SetActive(false);
        }
    }
}
