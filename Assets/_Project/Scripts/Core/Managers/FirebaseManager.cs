using System;
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
        // Firestore Paths
        private const string UsersCollection = "users";
        private const string GameDataCollection = "game_data";
        private const string LetterHuntDoc = "letter_hunt";
        private const string DrawLetterDoc = "draw_letter";
        private const string ObjectDetectionDoc = "object_detection";
        private const string PlayerDataDoc = "player_data";

        private ListenerRegistration drawLetterListener;
        private ListenerRegistration letterHuntListener;
        private ListenerRegistration objectDetectionListener;
        private ListenerRegistration playerDataListener;

        public FirebaseAuth Auth { get; private set; }
        public FirebaseFirestore Firestore { get; private set; }
        public static FirebaseManager Instance { get; private set; }

        public bool IsInitialized { get; private set; }

        public static event Action OnFirebaseManagerReady;
        public event Action OnFirebaseInitialized;
        public event Action OnLetterHuntDataUpdated;
        public event Action OnDrawLetterDataUpdated;
        public event Action OnObjectDetectionDataUpdated;
        public event Action OnPlayerDataUpdated;

        private void Awake()
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

        private void OnDestroy()
        {
            letterHuntListener?.Stop();
            playerDataListener?.Stop();
            objectDetectionListener?.Stop();
            drawLetterListener?.Stop();
        }



        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var app = FirebaseApp.DefaultInstance;
                Firestore = FirebaseFirestore.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;

                Debug.Log("Firebase initialized successfully.");
                OnFirebaseManagerReady?.Invoke();
                OnFirebaseInitialized?.Invoke();
                IsInitialized = true;
            });
        }

        #region Save Methods

        public async Task SaveLetterHuntData(LetterHuntDataSerializable serializableData)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var docRef = GetUserGameDocRef(LetterHuntDoc);
                await docRef.SetAsync(serializableData);
                Debug.Log("Letter Hunt data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Letter Hunt data: {e.Message}");
            }
        }

        public async Task SaveDrawLetterData(DrawLetterDataSerializable serializableData)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var docRef = GetUserGameDocRef(DrawLetterDoc);
                await docRef.SetAsync(serializableData);
                Debug.Log("Draw Letter data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Draw Letter data: {e.Message}");
            }
        }

        public async Task SaveObjectDetectionData(ObjectDetectionDataSerializable serializableData)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var docRef = GetUserGameDocRef(ObjectDetectionDoc);
                await docRef.SetAsync(serializableData);
                Debug.Log("Object Detection data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Object Detection data: {e.Message}");
            }
        }

        public async Task SavePlayerData(PlayerAbilityStatsDataSerializable serializableData)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var docRef = GetUserGameDocRef(PlayerDataDoc);
                await docRef.SetAsync(serializableData);
                Debug.Log("Player Ability Stats data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Player Ability Stats data: {e.Message}");
            }
        }

        #endregion

        #region Real-Time Listeners

        public void StartListeningForChanges()
        {
            if (!IsUserLoggedIn()) return;

            ListenToLetterHuntData();
            ListenToDrawLetterData();
            ListenToObjectDetectionData();
            ListenToPlayerData();
        }

        public void StopListeningForChanges()
        {
            letterHuntListener?.Stop();
            drawLetterListener?.Stop();
            objectDetectionListener?.Stop();
            playerDataListener?.Stop();
        }

        private void ListenToLetterHuntData()
        {
            var docRef = GetUserGameDocRef(LetterHuntDoc);

            letterHuntListener = docRef.Listen(snapshot =>
            {
                try
                {
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ConvertTo<LetterHuntDataSerializable>();
                        Debug.Log(
                            $"[REAL-TIME] Letter Hunt Updated - Correct: {data.correctScore}, Incorrect: {data.incorrectScore}");

                        OnLetterHuntDataUpdated?.Invoke();
                        PersistentSOManager.GetSO<LetterHuntData>()?.UpdateLocalData(data);
                    }
                    else
                    {
                        Debug.LogWarning("No Letter Hunt data found. Creating default...");
                        var defaultData = new LetterHuntDataSerializable(PersistentSOManager.GetSO<LetterHuntData>());
                        docRef.SetAsync(defaultData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Firestore Listen Error (Letter Hunt): {e.Message}");
                }
            });
        }

        private void ListenToDrawLetterData()
        {
            var docRef = GetUserGameDocRef(DrawLetterDoc);

            drawLetterListener = docRef.Listen(snapshot =>
            {
                try
                {
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ConvertTo<DrawLetterDataSerializable>();
                        Debug.Log(
                            $"[REAL-TIME] Draw Letter Updated - Correct: {data.correctScore}, Incorrect: {data.incorrectScore}");

                        OnDrawLetterDataUpdated?.Invoke();
                        PersistentSOManager.GetSO<DrawLetterData>()?.UpdateLocalData(data);
                    }
                    else
                    {
                        Debug.LogWarning("No Draw Letter data found. Creating default...");
                        var defaultData = new DrawLetterDataSerializable(PersistentSOManager.GetSO<DrawLetterData>());
                        docRef.SetAsync(defaultData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Firestore Listen Error (Draw Letter): {e.Message}");
                }
            });
        }

        private void ListenToObjectDetectionData()
        {
            var docRef = GetUserGameDocRef(ObjectDetectionDoc);

            objectDetectionListener = docRef.Listen(snapshot =>
            {
                try
                {
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ConvertTo<ObjectDetectionDataSerializable>();
                        Debug.Log(
                            $"[REAL-TIME] Object Detection Updated - Correct: {data.correctScore}, Incorrect: {data.incorrectScore}");

                        OnObjectDetectionDataUpdated?.Invoke();
                        PersistentSOManager.GetSO<ObjectDetectionData>()?.UpdateLocalData(data);
                    }
                    else
                    {
                        Debug.LogWarning("No Object Detection data found. Creating default...");
                        var defaultData =
                            new ObjectDetectionDataSerializable(PersistentSOManager.GetSO<ObjectDetectionData>());
                        docRef.SetAsync(defaultData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Firestore Listen Error (Object Detection): {e.Message}");
                }
            });
        }

        private void ListenToPlayerData()
        {
            var docRef = GetUserGameDocRef(PlayerDataDoc);

            playerDataListener = docRef.Listen(snapshot =>
            {
                try
                {
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ConvertTo<PlayerAbilityStatsDataSerializable>();
                        Debug.Log($"[REAL-TIME] Player Stats Updated - Energy Points: {data.energyPoints}");

                        OnPlayerDataUpdated?.Invoke();
                        PersistentSOManager.GetSO<PlayerAbilityStats>().UpdateLocalData(data);
                    }
                    else
                    {
                        Debug.LogWarning("No Player data found. Creating default...");
                        var defaultData =
                            new PlayerAbilityStatsDataSerializable(PersistentSOManager.GetSO<PlayerAbilityStats>());
                        docRef.SetAsync(defaultData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Firestore Listen Error (Player Data): {e.Message}");
                }
            });
        }

        #endregion

        #region Helpers

        private bool IsUserLoggedIn()
        {
            if (Auth.CurrentUser == null)
            {
                Debug.LogWarning("Operation failed: User is not logged in.");
                return false;
            }

            return true;
        }

        private DocumentReference GetUserGameDocRef(string documentId)
        {
            if (Auth.CurrentUser?.UserId == null)
            {
                Debug.LogWarning("User ID is null. Cannot proceed.");
                return null;
            }
            return Firestore.Collection(UsersCollection)
                .Document(Auth.CurrentUser.UserId)
                .Collection(GameDataCollection)
                .Document(documentId);
        }

        #endregion
    }
}