using System;
using System.Threading.Tasks;
using _Project.Scripts.Core.SaveSystem;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace _Project.Scripts.Core.Managers
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        private FirebaseFirestore _firestore;
        private ListenerRegistration listener;

        public event Action OnLetterHuntDataUpdated;

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
                StartListeningForChanges();
            });
        }

        public async Task SaveLetterHuntData(LetterHuntData data)
        {
            try
            {
                LetterHuntDataSerializable serializableData = new LetterHuntDataSerializable(data);
                DocumentReference docRef = _firestore.Collection("game_data").Document("letter_hunt");
                await docRef.SetAsync(serializableData);
                Debug.Log("Letter Hunt data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Letter Hunt data: {e.Message}");
            }
        }

        private void StartListeningForChanges()
        {
            DocumentReference docRef = _firestore.Collection("game_data").Document("letter_hunt");

            listener = docRef.Listen(snapshot =>
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

                        docRef.SetAsync(defaultData);
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
