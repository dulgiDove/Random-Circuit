using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameplayUIBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private DashUI dashUI;
    [SerializeField]
    private InteractionUI interactionUI;
    [SerializeField]
    private CannonChargeUI cannonChargeUI;

    private void Awake()
    {
        Debug.Assert(dashUI != null);
        Debug.Assert(interactionUI != null);
        Debug.Assert(cannonChargeUI != null);
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() =>
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsClient &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null
        );

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();
        PlayerInteraction interaction = playerObject.GetComponent<PlayerInteraction>();

        dashUI.Bind(movement);
        interactionUI.Bind(interaction);
        cannonChargeUI.Bind(interaction);
    }
}