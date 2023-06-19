using UnityEngine;
using UnityEngine.AI;
public class BotMovement : MonoBehaviour
{
    float botTooCloseRange = 1.5f;                                  // At which range has the bot reached it's target, and it should stop moving towards it.
    [HideInInspector] public NavMeshAgent navMeshBot = null;        // NavMeshBot component
    [HideInInspector] public Animator botAnimator = null;           // Bot Animator

    [Header("Debugging")]
    [SerializeField] bool enableDebugging = true;                   // Enable or disable BotMovement debugging

    /// <summary> Movement logic for the bot, which will also apply speed boost incase the target is far away</summary>
    public virtual void Move(Vector3 position)
    {
        if (!enabled) return;                                                          // Is component disabled then return
        if (Vector3.Distance(position, transform.position) < botTooCloseRange) return; // Too close, just stop and attack or idle w/e

        navMeshBot.SetDestination(position);

        if(enableDebugging)
            Functions.DebugMessage($"Moving {gameObject.name} (Distance to destination: {Vector3.Distance(transform.position, position)})", Functions.DebugTypes.INFO);
    }

    private void FixedUpdate()
    {
        botAnimator.SetFloat("Velocity", navMeshBot.velocity.magnitude / navMeshBot.speed);
    }
}
