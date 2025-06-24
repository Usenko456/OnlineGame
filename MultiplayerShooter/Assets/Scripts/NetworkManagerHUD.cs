using Unity.Netcode;
using UnityEngine;

public class NetworkManagerHUD : MonoBehaviour
{
    private bool showGUI = true;       // Flag to toggle GUI visibility
    private string ip = "127.0.0.1";   // Default IP address for client connection

    void OnGUI()
    {
        // Exit if there is no NetworkManager instance
        if (NetworkManager.Singleton == null)
            return;
        if (!showGUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150)); // Define GUI area
        GUILayout.Label("IP address (for client):");
        ip = GUILayout.TextField(ip);                     // Text field to enter IP

        // If neither client nor server is running, show Host and Client buttons
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();    // Start as host (server + client)
            }

            if (GUILayout.Button("Client"))
            {
                // Set the client connection IP address
                NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().ConnectionData.Address = ip;
                NetworkManager.Singleton.StartClient();  // Start as client
            }
        }
        else
        {
            // If already connected, show Shutdown button to stop network session
            if (GUILayout.Button("Shutdown"))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}
