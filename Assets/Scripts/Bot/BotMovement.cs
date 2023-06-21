using UnityEngine;
using UnityEngine.AI;
public class BotMovement : MonoBehaviour
{
    float botTooCloseRange = 1.5f;                                  // At which range has the bot reached it's target, and it should stop moving towards it.
    [HideInInspector] public NavMeshAgent navMeshBot = null;        // NavMeshBot component
    [HideInInspector] public Animator botAnimator = null;           // Bot Animator

    [Header("Debugging")]
    [SerializeField] public bool enableDebugging = true;                   // Enable or disable BotMovement debugging

    /// <summary> Movement logic for the bot, which will also apply speed boost incase the target is far away</summary>
    public virtual void Move(Vector3 position, bool isClientMove = false)
    {
        if (!enabled) return;                                                          // Is component disabled then return
        if (Vector3.Distance(position, transform.position) < botTooCloseRange) return; // Too close, just stop and attack or idle w/e

        if (navMeshBot.SetDestination(position))
        {
            if(isClientMove)
            {
                GetComponent<BaseBot>().ResetCombat();
                GetComponent<BaseBot>().orderedPosition = position;
            }
        }

        if(enableDebugging)
            Functions.DebugMessage($"Moving {gameObject.name} (Distance to destination: {Vector3.Distance(transform.position, position)})", Functions.DebugTypes.INFO);
    }

    /// <summary> Set the animator Velocity here for walk animation</summary>
    private void FixedUpdate()
    {
        botAnimator.SetFloat("Velocity", navMeshBot.velocity.magnitude / navMeshBot.speed);
    }
}
