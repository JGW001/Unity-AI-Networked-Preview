using UnityEngine;

public class BotAnimator : MonoBehaviour
{
    [HideInInspector] public Animator animator = null;

    public void AnimationTrigger(byte animTrigger)
    {
        Functions.AnimationType triggerType = (Functions.AnimationType)animTrigger;
        switch (triggerType)
        {
            case Functions.AnimationType.Jump:
                animator.SetTrigger("Jump");
                break;

            case Functions.AnimationType.Attack:
                int randomAttack = (int)Random.Range(0f, 10f);

                if(randomAttack > 4)
                    animator.SetTrigger("Punch");
                else
                    animator.SetTrigger("Kick");
                break;

        }
    }
}
