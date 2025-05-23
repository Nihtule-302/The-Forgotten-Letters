using UnityEngine;

namespace TheForgottenLetters._Project.Art.Art_Assets.Icons._Heathen_Engineering.Samples.UX_Examples.Flat_Icons__Free_
{
    public class ToggleSetAnimatorBoolean : MonoBehaviour
    {
        public Animator animator;
        public string booleanName;

        public void SetBoolean(bool value)
        {
            animator.SetBool(booleanName, value);
        }
    }
}
