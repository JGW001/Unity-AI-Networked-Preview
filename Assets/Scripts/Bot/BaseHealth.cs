using UnityEngine;
using Unity.Netcode;

/// <summary> The BaseHealth class which does the damage dealing, ressurrection of player & activates ragdoll</summary>
public class BaseHealth : NetworkBehaviour
{
    [SerializeField] public int botHealth;    // The health of the bot

    public virtual void Start()
    {
        botHealth = 500;
    }

    public virtual void TakeDamage(int amount)
    {
        botHealth -= amount;

        if(botHealth <= 0)
        {
            ExecuteEntityClientRpc();

            if(IsServer)
            {
                Invoke("ResurrectPlayer", 5f);      // Only run this on BaseHealth for the player, zombies should not respawn but instead destroy (This is overridden in HealthZombie)
            }
        }
    }

    [ClientRpc]
    public virtual void ExecuteEntityClientRpc()
    {
        gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        if (gameObject.GetComponent<BaseBot>().enableRagdoll)
            gameObject.GetComponent<Animator>().enabled = false;
        else gameObject.GetComponent<Animator>().SetBool("isDead", true);

        gameObject.GetComponent<BaseBot>().enabled = false;
    }
    private void ResurrectPlayer()
    {
        ResurrectPlayerClientRpc();
    }

    [ClientRpc]
    public virtual void ResurrectPlayerClientRpc()
    {
        gameObject.GetComponent<BaseBot>().enabled = true;
        gameObject.GetComponent<Collider>().enabled = true;
        gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
        gameObject.GetComponent<BaseHealth>().botHealth = 300;

        if (gameObject.GetComponent<BaseBot>().enableRagdoll)
            gameObject.GetComponent<Animator>().enabled = true;
        else gameObject.GetComponent<Animator>().SetBool("isDead", false);
    }
}
