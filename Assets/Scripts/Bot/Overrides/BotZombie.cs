using System.Collections.Generic;
using UnityEngine;
/// <summary> A derived BaseBot class for the zombies</summary>
public class BotZombie : BaseBot
{
    public List<string> botNames;

    #region Overridden functions
    // Overridden InitializeBot to change some values so they don't behave exactly like humans.
    public override void InitializeBot()
    {
        if (IsServer)
        {
            botName.Value = botNames[Random.Range(0, botNames.Count)];

            if(botCombat)
            {
                // We change some variables for the zombies, so they aren't exactly as the humans
                botCombat.targetOutOfRangeDistance = 25f;   // Increase "Out Of Sight/Range" for zombies
                botCombat.sphereCastRadius = 12;            // Increase the detection range of humans for zombies
            }

            botSkin.Value = (uint)Random.Range(0, botMaterialSkins.Length);
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = botMaterialSkins[(int)botSkin.Value];
        }
        else transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = botMaterialSkins[(int)botSkin.Value];

        botNameTag.text = botName.Value;
    }

    // Overridden OnStateEnter for the zombies, to reduce their "next action" time, so they are a bit more "alive"
    public override void OnStateEnter(Functions.BotState newState, Functions.BotState oldState)
    {
        switch (newState)   // Which state it is entering
        {
            case Functions.BotState.Start:
            {
                brainReactionTime = 0.5f;
                break;
            }

            case Functions.BotState.Idle:
            {
                brainReactionTime = Random.Range(1f, 3f);
                break;
            }

            case Functions.BotState.Wander:
            {
                brainReactionTime = Random.Range(0.5f, 3f);
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
    #endregion
}
