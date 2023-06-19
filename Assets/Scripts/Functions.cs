using UnityEngine;
public class Functions : MonoBehaviour
{
    public enum BotState
    {
        Start,

        Idle,
        Wander,

        Follow,

        Attack,
        Flee,
    }

    public enum ConnectionType
    {
        Client = 0,
        Host = 1,
        Server = 2,
    }

    public enum DebugTypes
    {
        ERROR = 0,
        SUCCESS = 1,
        INFO = 2,
        DEBUG = 3,
    }

    public enum AnimationType
    {
        None = 0,
        Jump = 1,
        Attack = 2,

        CrouchWalk = 4,
        CrouchRun = 5,
        Shoot = 6,
    }

    /// <summary> Better debug messages</summary>
    public static void DebugMessage(string errorMsg, DebugTypes debugType = DebugTypes.INFO)
    {
        switch (debugType)
        {
            case DebugTypes.ERROR:
                Debug.Log("<color=red>ERROR: </color>" + errorMsg);
                break;

            case DebugTypes.SUCCESS:
                Debug.Log("<color=green>SUCCESS: </color>" + errorMsg);
                break;

            case DebugTypes.INFO:
                Debug.Log("<color=yellow>INFO: </color>" + errorMsg);
                break;

            case DebugTypes.DEBUG:
                Debug.Log("<color=purple>DEBUG: </color>" + errorMsg);
                break;
        }

        return;
    }

    public static Vector3 GetRandomPositionWithinDistance(Transform transform, float distance)
    {
        float offsetX = Random.Range(-distance, distance);
        float offsetZ = Random.Range(-distance, distance);

        return new Vector3(transform.position.x + offsetX, 0, transform.position.z + offsetZ);
    }
}
