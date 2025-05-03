using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;

namespace TheForgottenLetters
{
    public class Auth : MonoBehaviour
    {

        public TMP_InputField emailInputField;
        public TMP_InputField passwordInputField;
        public TMP_InputField noteInputField; // Add input field for the note
        public TMP_Text statusText;

        private FirebaseAuth auth;
        private FirebaseFirestore db; // Add Firestore instance variable

        void Start()
        {
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.Auth != null)
            {
                auth = FirebaseManager.Instance.Auth;
                db = FirebaseManager.Instance.Firestore; // Initialize Firestore
            }
            else
            {
                Debug.LogError("FirebaseManager or Firebase Auth is not initialized.");
                UpdateStatus("Firebase is not ready.");
            }

            if (auth.CurrentUser != null)
            {
                // User is already signed in, you can update the UI accordingly
                UpdateStatus("User is already signed in: " + auth.CurrentUser.Email);
                FirebaseManager.Instance.StartListeningForChanges();
            }
            else
            {
                UpdateStatus("No user is currently signed in.");
            }
        }

        // Helper to update status text on the main thread
        void UpdateStatus(string message)
        {
            statusText.text = message;
            Debug.Log(message); // Also log to console
        }

        public async void SignUp()
        {
            string email = emailInputField.text;
            string password = passwordInputField.text;

            try
            {
                await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                UpdateStatus("Sign Up Successful!");
                FirebaseManager.Instance.StartListeningForChanges();
                SaveAuthInfo(); // Save user info after sign up
            }
            catch (System.Exception ex)
            {
                UpdateStatus("Sign Up Failed: " + ex.Message);
            }
        }

        public async void Login()
        {
            string email = emailInputField.text;
            string password = passwordInputField.text;

            try
            {
                await auth.SignInWithEmailAndPasswordAsync(email, password);
                UpdateStatus("Login Successful!");
                FirebaseManager.Instance.StartListeningForChanges();
                SaveAuthInfo(); // Save user info after login
            }
            catch (System.Exception ex)
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
                UpdateStatus("Error: Must be logged in to save a note.");
                return;
            }
            if (db == null) {
                UpdateStatus("Error: Firestore not initialized.");
                return;
            }

            string userId = auth.CurrentUser.UserId;

            // Create a document reference using the user's ID
            // This stores the note under a collection 'userNotes' in a document named after the user's ID
            DocumentReference docRef = db.Collection("users").Document(userId);

            // Create data to save. Using a Dictionary.
            // We add the note, the userId, and a server timestamp.
            Dictionary<string, object> noteData = new Dictionary<string, object>
            {
                { "userId", userId }, 
                { "email", auth.CurrentUser.Email } 
            };

            // Use SetAsync with MergeAll to create or update the document
            // This won't overwrite other fields if the document already exists
            UpdateStatus("Saving Data...");
            docRef.SetAsync(noteData, SetOptions.MergeAll).ContinueWithOnMainThread(task => { // Ensure this line uses the extension method
                if (task.IsCompletedSuccessfully)
                {
                    UpdateStatus("Data saved successfully!");
                }
                else if (task.IsFaulted)
                {
                    UpdateStatus($"Error saving Data: {task.Exception}");
                }
                else if (task.IsCanceled) {
                    UpdateStatus("Saving Data was cancelled.");
                }
            });
        }
    }
}
