using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using Firebase.Firestore;
using UnityEngine;

namespace TheForgottenLetters
{
    [CreateAssetMenu(fileName = "PlayerSkills", menuName = "Player/PlayerSkills")]
    public class PlayerSkills : ScriptableObject
    {
        [SerializeField] private List<string> unlockedSkills_names = new();
        [SerializeField] private List<Skill> unlockedSkills = new();

        public List<string> UnlockedSkills_names
        {
            get
            {
                SaveSkillsToNames();
                return unlockedSkills_names;
            }
            set
            {
                unlockedSkills_names = value;
                LoadSkillsFromNames();
            }
        }

        public List<Skill> UnlockedSkills
        {
            get
            {
                LoadSkillsFromNames();
                return unlockedSkills;
            }
            set
            {
                unlockedSkills = value;
                SaveSkillsToNames();
            }
        }

        private SkillManager skillManager => SkillManager.Instance;

        public void LoadSkillsFromNames()
        {
            unlockedSkills = skillManager.ConvertNamesToSkills(unlockedSkills_names);
        }

        public void SaveSkillsToNames()
        {
            unlockedSkills_names.Clear();
            foreach (var skill in
                     unlockedSkills) unlockedSkills_names.Add(skill.name); // Assuming each Skill's name is unique
        }

        [ContextMenu("Reset Data")]
        public void ResetData()
        {
            unlockedSkills_names.Clear();
            unlockedSkills.Clear();
        }

        public void UpdateData(PlayerSkillsSerializable playerSkillsSerializable)
        {
            unlockedSkills_names = playerSkillsSerializable.unlockedSkills_names;
            LoadSkillsFromNames();
        }

        public void UpdateData(List<string> playerSkills_names)
        {
            unlockedSkills_names = playerSkills_names;
            LoadSkillsFromNames();
        }
    }

    [FirestoreData]
    public class PlayerSkillsSerializable
    {
        // ✅ Default constructor required for Firestore
        public PlayerSkillsSerializable()
        {
            unlockedSkills_names = new List<string>();
        }

        // ✅ Constructor for conversion from LetterHuntData
        public PlayerSkillsSerializable(PlayerSkills data)
        {
            unlockedSkills_names = new List<string>(data.UnlockedSkills_names);
        }

        [FirestoreProperty] public List<string> unlockedSkills_names { get; set; }
    }
}