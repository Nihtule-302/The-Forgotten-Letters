using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core.Managers
{
    public class SkillManager : MonoBehaviour
    {
        public List<Skill> allSkills = new();
        private readonly Dictionary<string, Skill> skillLookup = new();
        public static SkillManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keep across scenes
            BuildSkillLookup();
        }

        private void BuildSkillLookup()
        {
            skillLookup.Clear();
            foreach (var skill in allSkills)
                if (!skillLookup.ContainsKey(skill.name))
                    skillLookup.Add(skill.name, skill);
                else
                    Debug.LogWarning($"Duplicate skill name found: {skill.name}");
        }

        public Skill GetSkillByName(string skillName)
        {
            if (skillLookup.TryGetValue(skillName, out var skill)) return skill;
            Debug.LogWarning($"Skill with name '{skillName}' not found.");
            return null;
        }

        public List<Skill> ConvertNamesToSkills(List<string> skillNames)
        {
            var result = new List<Skill>();
            foreach (var name in skillNames)
            {
                var skill = GetSkillByName(name);
                if (skill != null) result.Add(skill);
            }

            return result;
        }
    }
}