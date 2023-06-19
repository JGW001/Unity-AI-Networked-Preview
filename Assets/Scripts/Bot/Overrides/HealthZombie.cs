using Unity.Netcode;
using UnityEngine;

public class HealthZombie : BaseHealth
{
    public override void Start()
    {
        botHealth = 10;        // Zombies have lower amount of HP
    }

    public override void TakeDamage(int amount)
    {
        botHealth -= amount;

        if (botHealth <= 0)
        {
            ExecuteEntityClientRpc();

            if(IsServer)
            {
                Destroy(gameObject, 10f);   // On the server side we will destroy the zombie after X seconds, which will also automatically sync with clients
            }
        }
    }

    [ClientRpc]
    public override void ExecuteEntityClientRpc()
    {
        base.ExecuteEntityClientRpc();     // Call base for ragdoll logic
    }
}
