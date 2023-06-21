using Unity.Netcode;
using UnityEngine;

public class BotPlayer : BaseBot
{
    // We override FixedUpdate for player owned bots, to allow them to send move requests
    public override void FixedUpdate()
    {
        // The server controls the combat & thinking
        if(IsServer)
        {
            BrainTimer();
        }

        // The owner of the object can request moving
        if(IsOwner)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RequestMoveServerRpc(Functions.GetMouseWorldPosition());
            }
        }
    }

    // Request the moving of the player to the server
    [ServerRpc]
    public void RequestMoveServerRpc(Vector3 newPosition)
    {
        if(newPosition == Vector3.zero)
        {
            print("RequestMoveServerRpc returned Vector3.zero");
            return;
        }

        if (botMovement)
            botMovement.Move(newPosition, true);
    }
}
