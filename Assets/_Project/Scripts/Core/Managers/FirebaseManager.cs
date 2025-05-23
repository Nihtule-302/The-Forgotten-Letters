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
        public static FirebaseManager Instance { get; private set; }
        public FirebaseAuth Auth { get; private set; }

        public FirebaseFirestore Firestore { get; private set; }

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

        public event Action OnLetterHuntDataUpdated;
        public event Action OnDrawLetterDataUpdated;
        public event Action OnObjectDetectionDataUpdated;
        public event Action OnPlayerDataUpdated;
        public event Action OnFirebaseInitialized;

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var app = FirebaseApp.DefaultInstance;
                Firestore = FirebaseFirestore.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;

                Debug.Log("Firebase initialized successfully.");
                OnFirebaseInitialized?.Invoke();
            });
        }

        #region Save Methods

        public async Task SaveLetterHuntData(LetterHuntData data)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var serializableData = new LetterHuntDataSerializable(data);
                var docRef = GetUserGameDocRef(LetterHuntDoc);
                await docRef.SetAsync(serializableData);
                Debug.Log("Letter Hunt data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Letter Hunt data: {e.Message}");
            }
        }

        public async Task SaveDrawLetterData(DrawLetterData data)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var serializableData = new DrawLetterDataSerializable(data);
                var docRef = GetUserGameDocRef(DrawLetterDoc);
                await docRef.SetAsync(serializableData);
                Debug.Log("Draw Letter data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Draw Letter data: {e.Message}");
            }
        }

        public async Task SaveObjectDetectionData(ObjectDetectionData data)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var serializableData = new ObjectDetectionDataSerializable(data);
                var docRef = GetUserGameDocRef(ObjectDetectionDoc);
                await docRef.SetAsync(serializableData);
                Debug.Log("Object Detection data saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving Object Detection data: {e.Message}");
            }
        }

        public async Task SavePlayerData(PlayerAbilityStats data)
        {
            if (!IsUserLoggedIn()) return;

            try
            {
                var serializableData = new PlayerAbilityStatsDataSerializable(data);
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
                        PersistentSOManager.GetSO<LetterHuntData>()?.UpdateData(data);
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
                        PersistentSOManager.GetSO<DrawLetterData>()?.UpdateData(data);
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
                        PersistentSOManager.GetSO<ObjectDetectionData>()?.UpdateData(data);
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
                        var databuilder = PersistentSOManager.GetSO<PlayerAbilityStats>().GetBuilder();
                        databuilder
                            .SetEnergyPoints(data.energyPoints)
                            .SetSkills(data.unlockedSkillNames)
                            .SetlastTimeEnergyIncreased(data.lastTimeEnergyIncreasedCairoTime);

                        PersistentSOManager.GetSO<PlayerAbilityStats>().UpdateData(databuilder);
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
            return Firestore.Collection(UsersCollection)
                .Document(Auth.CurrentUser.UserId)
                .Collection(GameDataCollection)
                .Document(documentId);
        }

        #endregion
    }
}