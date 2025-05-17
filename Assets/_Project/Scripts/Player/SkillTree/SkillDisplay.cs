using System;
using _Project.Scripts.Core.Managers;
using TheForgottenLetters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDisplay : MonoBehaviour
{
    public Skill skill;
    public Image backgroundImage;

    public Animator skillAnimator;

    public Color lockedColor;
    public Color unlockableColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnableSkill();
    }

    void OnEnable()
    {
        EnableSkill();
    }

    public void EnableSkill()
    {
        if (skill.IsSkillUnlocked())
        {
            SkillActivated();
        }
        else if(!skill.CanUnlockSkill())
        {
            SkillLocked();
        }
        else
        {
            SkillUnlockable();
        }
    }

    private void SkillLocked()
    {
        backgroundImage.color = lockedColor;
        this.gameObject.GetComponent<Button>().interactable = false;
        skillAnimator.enabled = false;
    }

    private void SkillUnlockable()
    {
        backgroundImage.color = unlockableColor;
        this.gameObject.GetComponent<Button>().interactable = true;
        skillAnimator.enabled = false;
    }

    private void SkillActivated()
    {
        backgroundImage.color = unlockableColor;
        this.gameObject.GetComponent<Button>().interactable = false;
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
