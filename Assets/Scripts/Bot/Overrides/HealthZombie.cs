using Unity.Netcode;
using DG.Tweening;
using UnityEngine;

public class HealthZombie : BaseHealth
{
    public override void Start()
    {
        botHealth = 15;        // Zombies have lower amount of HP
    }

    public override void TakeDamage(int amount)
    {
        botHealth -= amount;

        if (botHealth <= 0)
        {
            ExecuteEntityClientRpc();

            if(IsServer)
            {
                GetComponent<BaseBot>().killCounter.IncreaseCounter();
                Destroy(gameObject, 10f);   // On the server side we will destroy the zombie after X seconds, which will also automatically sync with clients
            }
        }
        else TakeDamageClientRpc((uint)amount);
    }

    [ClientRpc]
    public override void TakeDamageClientRpc(uint amount)
    {
        botHealth = (int)amount;
        gameObject.transform.DOShakeScale(.2f, 0.1f).OnComplete(ResetScale);
    }

    [ClientRpc]
    public override void ExecuteEntityClientRpc()
    {
        gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        if (gameObject.GetComponent<BaseBot>().enableRagdoll)
            gameObject.GetComponent<Animator>().enabled = false;
        else gameObject.GetComponent<Animator>().SetBool("isDead", true);

        gameObject.GetComponent<BaseBot>().enabled = false;
    }
}
