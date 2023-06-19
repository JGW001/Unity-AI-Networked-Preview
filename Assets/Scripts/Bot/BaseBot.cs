using UnityEngine;
using Unity.Netcode;

/// <summary> The core of the bots thinking, this is derived for zombies</summary>
public class BaseBot : NetworkBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] bool enableMovement = true;                    // If set to true, movement logic will be applied upon spawning of the AI
    [SerializeField] bool enableCombat = true;                      // If set to true, combat logic will be applied upon spawning of the AI
    [SerializeField] public bool enableRagdoll = false;             // Enable / Disable ragdoll, ragdolls are extremely expensive & slow to network so I left them clientsided, can switch to a death animation instead.

    [Header("Bot Variables")]
    Functions.BotState currentBotState = Functions.BotState.Idle;   // The current state of the bot
    [SerializeField] float brainTimer = 0f;                         // The current saved brain time 
    [SerializeField] protected float brainReactionTime = 0f;        // The time needed before brain (thinking) triggers
    bool brainActive = true;                                        // Is the AI brain active or not, setting to false will completely stop calculations.
    bool isInCombat = false;                                        // Is the bot in combat?
    protected bool isCloseToTarget = false;                         // Is the bot close to its target?
    [SerializeField] public Transform currentTarget = null;                // The current target of the bot

    float currentOutOfCombatTime = 0f;                              // Saved amount of seconds the bot has been out of combat
    float outOfCombatTime = 25f;                                    // Amount of seconds to be out of combat before heading to the center of map for combat
    float _savedBrainReactionTime;

    [Header("Bot Skins")]
    [SerializeField] public Material[] botMaterialSkins = null;

    // Components, extras
    protected BotCombat botCombat = null;                             // botCombat componen
    protected BotMovement botMovement = null;                         // botMovement component

    // Networked Variables
    /// <summary> The skin of the zombie, to synchronize with connected clients, we use uint to reduce packet size.</summary>
    [SerializeField] protected NetworkVariable<uint> botSkin = new NetworkVariable<uint>();

    #region Standard functions (Awake, Update etc)
    /// <summary> This will run immediately when the object has been spawned over the network</summary>
    public override void OnNetworkSpawn()
    {
        // This will run on clients & server/host
        if(enableCombat)
        {
            botCombat = gameObject.AddComponent(typeof(BotCombat)) as BotCombat;        // Add combat component
            botCombat.botAnimator = GetComponent<BotAnimator>();                        // Attach the botanimator
            botCombat.botAnimator.animator = GetComponent<Animator>();                  // Attach the animator to the custom bot animator script
        }

        // What only needs to be run on the server.
        if(IsServer)
        {
            // The start state has a faster brain reaction than the other states, so next move is calculated faster
            OnStateChange(Functions.BotState.Start);

            if (enableMovement)
            {
                botMovement = gameObject.AddComponent(typeof(BotMovement)) as BotMovement;  // Add movement component
                botMovement.botAnimator = GetComponent<Animator>();                         // Attach animator component
                botMovement.navMeshBot = GetComponent<UnityEngine.AI.NavMeshAgent>();       // Attach navmeshagent component
            }

            _savedBrainReactionTime = brainReactionTime;
            outOfCombatTime = (int)Random.Range(15, 40);
        }
        else
        {
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        }

        // IntializeBot will setup the skin, and do some combat timing changes on the serverside for Zombies (See overridden InitializeBot in BotZombie.cs)
        InitializeBot();
    }

    public virtual void FixedUpdate()
    {
        // Stop FixedUpdate here if not server, or if brain is not active
        if (!IsServer || !brainActive) return;

        // The logic of the bot is done in BrainTimer
        BrainTimer();
    }
    #endregion

    #region Brain, calculations
    /// <summary> This is just a timer to trigger the brain logic</summary>
    public virtual void BrainTimer()
    {
        // For smoother rotation/look at target
        if (currentTarget != null)
        {
            LookAtTarget(currentTarget);
        }

        // Bots start in the Start state.
        if (currentBotState == Functions.BotState.Start)
        {
            OnStateChange(Functions.BotState.Wander);
            return;
        }

        // If bots wander too far away for too long, send them back to the center of map for combat
        if(currentBotState != Functions.BotState.Follow && currentBotState != Functions.BotState.Attack)
        {
            currentOutOfCombatTime += Time.deltaTime;
            if(currentOutOfCombatTime > outOfCombatTime)
            {
                brainTimer = 0f;
                botMovement.Move(new Vector3(0, 0, 0));
                if (Vector3.Distance(new Vector3(0, 0, 0), transform.position) < 10)
                {
                    currentOutOfCombatTime = 0;
                }
            }
        }

        // Brain timer
        brainTimer += Time.deltaTime;
        if (brainTimer >= brainReactionTime)
            Brain();
    }

    /// <summary> This is what is triggered from the timer, this is where all logic comes from</summary>
    public virtual void Brain()
    {
        brainTimer = 0f;

        // Check if the bot is in combat & has the combat logic and if its close enough to its target, else move towards it.
        if(isInCombat && botCombat) 
        {
            
            if(currentTarget == null)
            {
                OnStateChange(Functions.BotState.Idle);
                currentTarget = null;
                isInCombat = false;
            }

            if (Vector3.Distance(transform.position, currentTarget.position) < botCombat.minAttackRange)
            {
                isCloseToTarget = true;

                // If bot was in other state, transistion to attack state to set proper values else attack
                if (currentBotState != Functions.BotState.Attack)
                    OnStateChange(Functions.BotState.Attack);
                else
                {
                    // Before we perform an attack, check if the target is alive
                    if (!botCombat.GetTargetStatus(currentTarget.gameObject))
                    {
                        botCombat.CleanUpTargets();
                        currentTarget = botCombat.BestTargetOfInterest();

                        if (currentTarget == null)
                        {
                            OnStateChange(Functions.BotState.Idle);
                            currentTarget = null;
                            isInCombat = false;
                        }
                        else
                        {
                            // Head to the new target
                            OnStateChange(Functions.BotState.Follow);
                        }
                    }

                    // Check if the bot can attack, and then perform an attack
                    if (botCombat.CanAttack())
                        botCombat.StartAttack();
                }
            }
            else
            {
                isCloseToTarget = false;

                // First clean up interested targets, and move on.
                botCombat.CleanUpTargets();

                // If bot has no targets in the list of interest, leave combat
                if (botCombat.targetsOfInterest.Count == 0)
                {
                    Functions.DebugMessage($"{gameObject.name} did not have any targets in the list of interests, resetting combat", Functions.DebugTypes.ERROR);
                    ToggleCombatValues(false);
                    return;
                }

                // If current target is no longer in the list of interested targets, leave combat
                if(!botCombat.targetsOfInterest.Contains(currentTarget.gameObject))
                {
                    Functions.DebugMessage($"{gameObject.name} did not have {currentTarget.gameObject.name} as a target of interest, resetting combat", Functions.DebugTypes.ERROR);
                    ToggleCombatValues(false);
                    return;
                }

                // Even though the bot is in combat, we wanna see if any new threats has appeared, DetectEnemies automatically checks which nearby enemy is the best option.
                currentTarget = botCombat.DetectEnemies();

                // Set state incase it was not set & move to the target
                if(currentBotState != Functions.BotState.Follow) OnStateChange(Functions.BotState.Follow);
                botMovement.Move(currentTarget.position);
            }
        }

        // If not in combat & can move
        if(!isInCombat && botMovement)
        {
            // Bot is not in combat, so detect for enemies each brain tick
            currentTarget = botCombat.DetectEnemies();

            // If it found a target, enter combat else head to a random position (wander)
            if(currentTarget != null)
                ToggleCombatValues(true);
            else
            {
                int randomness = (int)Random.Range(0f, 10f);

                if(randomness > 5)
                {
                    if(currentBotState != Functions.BotState.Wander)
                    {
                        OnStateChange(Functions.BotState.Wander);
                    }

                    botMovement.Move(Functions.GetRandomPositionWithinDistance(transform, 10f));
                }
                else OnStateChange(Functions.BotState.Idle);
            }
        }
    }
    #endregion

    #region States
    /// <summary> Transitioning to other states</summary>
    public virtual void OnStateChange(Functions.BotState newState)
    {
        Functions.DebugMessage($"OnStateChange: {gameObject.name} is changing state from {(Functions.BotState)currentBotState} to {(Functions.BotState)newState}", Functions.DebugTypes.INFO);
        Functions.BotState _currentState = currentBotState;

        OnStateExit(_currentState, newState);           // OnStateExit event
        currentBotState = newState;                     // Set new state.
        OnStateEnter(newState, _currentState);          // OnStateEnter event
    }

    /// <summary> What happens when the bot enters different states</summary>
    public virtual void OnStateEnter(Functions.BotState newState, Functions.BotState oldState)
    {
        switch (newState)   // Which state it is entering
        {
            case Functions.BotState.Start:
            {
                brainReactionTime = Random.Range(0.2f, 1.5f);
                break;
            }

            case Functions.BotState.Idle:
            {
                brainReactionTime = Random.Range(1f, 5f);
                break;
            }

            case Functions.BotState.Wander:
            {
                brainReactionTime = Random.Range(1f, 7f);
                break;
            }

            case Functions.BotState.Follow:
            {
                brainReactionTime = 1f;
                break;
            }

            case Functions.BotState.Attack:
            {
                // Disable navMesh rotation, as we are close to the target and want to be sure we hit it using a custom rotation script
                isCloseToTarget = true;
                botMovement.navMeshBot.updateRotation = false;
                brainReactionTime = 0.5f;

                if (botCombat.CanAttack())
                    botCombat.StartAttack();
                break;
            }
        }
    }

    /// <summary> What happens when the bot exits different states</summary>
    public virtual void OnStateExit(Functions.BotState oldState, Functions.BotState newState)
    {
        switch (oldState)   // Which state it was exiting
        {
            case Functions.BotState.Attack:
            {
                botMovement.navMeshBot.updateRotation = true;
                isCloseToTarget = false;
                currentOutOfCombatTime = 0f;
                break;
            }

            case Functions.BotState.Follow:
            {
                currentOutOfCombatTime = 0f;
                break;
            }

        }

        // We reset the brain reaction time back to normal which we saved in Start() as it is different per state
        brainReactionTime = _savedBrainReactionTime;
    }
    #endregion

    #region Other stuff
    /// <summary> Attempts to taunt the target, incase the target has no target</summary>
    /// <param name="attacker">The target that attacked this entity</param>
    public virtual void Taunt(Transform attacker)
    {
        if(currentTarget == null)
        {
            currentTarget = attacker;
            isInCombat = true;
            OnStateChange(Functions.BotState.Attack);
        }
    }

    public virtual void InitializeBot()
    {
        if(IsServer)
        {
            botSkin.Value = (uint)Random.Range(0, botMaterialSkins.Length);
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = botMaterialSkins[(int)botSkin.Value];
        }
        else transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = botMaterialSkins[(int)botSkin.Value];
    }

    public virtual void ToggleCombatValues(bool toggle)
    {
        if(!toggle)
        {
            currentTarget = null;
            isCloseToTarget = false;
            isInCombat = false;
            brainReactionTime = 2f;
        }
        else
        {
            isInCombat = true;
            brainReactionTime = 1f;
        }
    }

    public void LookAtTarget(Transform target)
    {
        if (!isCloseToTarget) return;
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
    }
    #endregion
}