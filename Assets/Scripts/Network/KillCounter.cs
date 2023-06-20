using UnityEngine;
using Unity.Netcode;
using TMPro;

public class KillCounter : NetworkBehaviour
{
    /// <summary> Current zombie kills, we use uint to reduce packet size.</summary>
    [SerializeField] protected NetworkVariable<uint> zombieKills = new NetworkVariable<uint>(0);
    [SerializeField] private TextMeshProUGUI counterText = null;

    public override void OnNetworkSpawn()
    {
        zombieKills.OnValueChanged += UpdateCounter;
        counterText.text = $"Zombie Kills: {zombieKills.Value}";
    }

    private void UpdateCounter(uint previousValue, uint newValue)
    {
        counterText.text = $"Zombie Kills: {zombieKills.Value}";
    }

    public void IncreaseCounter()
    {
        zombieKills.Value += 1;
    }
}
