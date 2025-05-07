using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Core.SaveSystem;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TheForgottenLetters;
using UnityEngine;

namespace _Project.Scripts.Core.Managers
{
    public class FirebaseManager : MonoBehaviour
    {
        
        public static FirebaseManager Instance { get; private set; }

        private FirebaseAuth auth;
        public FirebaseAuth Auth => auth;
        public FirebaseFirestore Firestore => _firestore;
        private FirebaseFirestore _firestore;
        
        private ListenerRegistration listener;

        public event Action OnLetterHuntDataUpdated;
        public event Action OnPlayerDataUpdated;
        public event Action OnFirebaseInitialized;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeFirebase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeFirebase()
        {
            // Check and fix Firebase dependencies
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var app = FirebaseApp.DefaultInstance;

                // Initialize Firestore
                _firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore initialized successfully.");

                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Auth initialized successfully.");

                OnFirebaseInitialized?.Invoke();
                Debug.Log("Firebase initialized successfully.");
            });
        }

        public async Task SaveLetterHuntData(LetterHuntData data)
        {
            if (auth.CurrentUser == null)
            {
                Debug.Log("Error: Must be logged in to save a note.");
                return;
            }
            try
            {
                LetterHuntDataSerializable serializableData = new LetterHuntDataSerializable(data);
                DocumentReference docRef = _firestore.Collection("users").Document(auth.CurrentUser.UserId).Collection("game_data").Document("letter_hunt");
                await docRef.SetAsync(serializableData);
                Debug.Log("Letter Hunt data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Letter Hunt data: {e.Message}");
            }
        }

        public async Task SavePlayerData(PlayerAbilityStats data)
        {
            if (auth.CurrentUser == null)
            {
                Debug.Log("Error: Must be logged in to save a note.");
                return;
            }
            try
            {
                PlayerAbilityStatsSerializable serializableData = new PlayerAbilityStatsSerializable(data);
                DocumentReference docRef = _firestore.Collection("users").Document(auth.CurrentUser.UserId).Collection("game_data").Document("player_data");
                await docRef.SetAsync(serializableData);
                Debug.Log("Player Ability Stats data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Player Ability Stats data: {e.Message}");
            }
        }

        public void StartListeningForChanges()
        {
            if (auth.CurrentUser == null)
            {
                Debug.Log("Error: Must be logged in to save a note."); // Also log to console
                return;
            }

            DocumentReference letterHuntDocRef = _firestore.Collection("users").Document(auth.CurrentUser.UserId).Collection("game_data").Document("letter_hunt");
           
            listener = letterHuntDocRef.Listen(snapshot =>
            {
                try
                {
                    if (snapshot.Exists)
                    {
                        LetterHuntDataSerializable data = snapshot.ConvertTo<LetterHuntDataSerializable>();
                        Debug.Log($"[REAL-TIME] Letter Hunt Data Updated - Correct: {data.correctScore}, Incorrect: {data.incorrectScore}");

                        OnLetterHuntDataUpdated?.Invoke();
                        PersistentSOManager.GetSO<LetterHuntData>().UpdateData(data);
                    }
                    else
                    {
                        Debug.LogWarning("No Letter Hunt data found. Creating default data...");
                        LetterHuntDataSerializable defaultData = new(PersistentSOManager.GetSO<LetterHuntData>());

                        letterHuntDocRef.SetAsync(defaultData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Firestore Listen Error: {e.Message}");
                }
            });

            DocumentReference playerDocRef = _firestore.Collection("users").Document(auth.CurrentUser.UserId).Collection("game_data").Document("player_data");
            listener = playerDocRef.Listen(snapshot =>
            {
                try
                {
                    if (snapshot.Exists)
                    {
                        PlayerAbilityStatsSerializable data = snapshot.ConvertTo<PlayerAbilityStatsSerializable>();
                        Debug.Log($"[REAL-TIME] Player Data Data Updated - Energy Points: {data.energyPoints}");

                        OnPlayerDataUpdated?.Invoke();
                        PersistentSOManager.GetSO<PlayerAbilityStats>().SetEnergyPoints(data.energyPoints);
                    }
                    else
                    {
                        Debug.LogWarning("No Player data found. Creating default data...");
                        PlayerAbilityStatsSerializable defaultData = new(PersistentSOManager.GetSO<PlayerAbilityStats>());

                        playerDocRef.SetAsync(defaultData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Firestore Listen Error: {e.Message}");
                }
            });

        }

        private void OnDestroy()
        {
            listener?.Stop();
        }
    }
}
