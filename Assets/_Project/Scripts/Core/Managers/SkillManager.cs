using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core.Managers
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        public List<Skill> allSkills = new List<Skill>();
        private Dictionary<string, Skill> skillLookup = new Dictionary<string, Skill>();

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
            {
                if (!skillLookup.ContainsKey(skill.name))
                {
                    skillLookup.Add(skill.name, skill);
                }
                else
                {
                    Debug.LogWarning($"Duplicate skill name found: {skill.name}");
                }
            }
        }

        public Skill GetSkillByName(string skillName)
        {
            if (skillLookup.TryGetValue(skillName, out Skill skill))
            {
                return skill;
            }
            Debug.LogWarning($"Skill with name '{skillName}' not found.");
            return null;
        }

        public List<Skill> ConvertNamesToSkills(List<string> skillNames)
        {
            List<Skill> result = new List<Skill>();
            foreach (string name in skillNames)
            {
                Skill skill = GetSkillByName(name);
                if (skill != null)
                {
                    result.Add(skill);
                }
            }
            return result;
        }
    }
}


