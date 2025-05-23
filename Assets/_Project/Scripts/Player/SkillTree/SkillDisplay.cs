using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Player.SkillTree
{
    public class SkillDisplay : MonoBehaviour
    {
        public Skill skill;
        public Image backgroundImage;

        public Animator skillAnimator;

        public Color lockedColor;
        public Color unlockableColor;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            EnableSkill();
        }

        private void OnEnable()
        {
            EnableSkill();
        }

        public void EnableSkill()
        {
            if (skill.IsSkillUnlocked())
                SkillActivated();
            else if (!skill.CanUnlockSkill())
                SkillLocked();
            else
                SkillUnlockable();
        }

        private void SkillLocked()
        {
            backgroundImage.color = lockedColor;
            gameObject.GetComponent<Button>().interactable = false;
            skillAnimator.enabled = false;
        }

        private void SkillUnlockable()
        {
            backgroundImage.color = unlockableColor;
            gameObject.GetComponent<Button>().interactable = true;
            skillAnimator.enabled = false;
        }

        private void SkillActivated()
        {
            backgroundImage.color = unlockableColor;
            gameObject.GetComponent<Button>().interactable = false;
            skillAnimator.enabled = true;
        }

        public void UnlockSkill()
        {
            if (skill.UnlockSkill())
            {
                SkillActivated();
                Debug.Log("Skill unlocked: " + skill.name);
            }
            else
            {
                Debug.Log("Skill not unlocked: " + skill.name);
            }
        }
    }
}