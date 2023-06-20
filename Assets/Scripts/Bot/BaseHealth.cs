using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// <summary> The BaseHealth class which does the damage dealing, ressurrection of player & activates ragdoll</summary>
public class BaseHealth : NetworkBehaviour
{
    [SerializeField] public int botHealth;                              // The health of the bot
    [SerializeField] public Slider healthSlider = null;                 // The UI Slider
    [SerializeField] protected TextMeshProUGUI healthText = null;       // The health text

    public virtual void Start()
    {
        botHealth = 100;
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            GameObject tmpCanvas = GameObject.Find("Canvas");

            healthSlider = tmpCanvas.transform.GetChild(1).GetComponent<Slider>();
            healthText = tmpCanvas.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        }
    }

    public virtual void TakeDamage(int amount)
    {
        botHealth -= amount;

        if (botHealth <= 0)
        {
            botHealth = 0;
            ExecuteEntityClientRpc();

            if (IsServer)
            {
                Invoke("ResurrectPlayer", 5f);      // Only run this on BaseHealth for the player, zombies should not respawn but instead destroy (This is overridden in HealthZombie)
            }
        }
        else TakeDamageClientRpc((uint)amount);
    }
    
    /// <summary>What the clients will see, when a entity takes damage, because as of now the health is serversided, so we will just make an effect on the target transform to indicate damage </summary>
    [ClientRpc]
    public virtual void TakeDamageClientRpc(uint amount)
    {
        gameObject.transform.DOShakeScale(.2f, 0.1f).OnComplete(ResetScale);

        if(IsOwner)
        {
            botHealth -= (int)amount;
            healthSlider.value = botHealth;
            healthText.text = $"{botHealth}";
        }
    }

    protected void ResetScale()
    {
        transform.localScale = new Vector3(1, 1, 1);
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

        if (IsOwner)
        {
            botHealth = 0;
            healthSlider.value = botHealth;
            healthText.text = $"{botHealth}";
        }
    }
    private void ResurrectPlayer()
    {
        ResurrectPlayerClientRpc();
    }

    [ClientRpc]
    public virtual void ResurrectPlayerClientRpc()
    {
        botHealth = 100;

        gameObject.GetComponent<BaseBot>().enabled = true;
        gameObject.GetComponent<Collider>().enabled = true;
        gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;

        if (gameObject.GetComponent<BaseBot>().enableRagdoll)
            gameObject.GetComponent<Animator>().enabled = true;
        else gameObject.GetComponent<Animator>().SetBool("isDead", false);

        if(IsOwner)
        {
            healthSlider.value = botHealth;
            healthText.text = $"{botHealth}";
        }
    }
}
