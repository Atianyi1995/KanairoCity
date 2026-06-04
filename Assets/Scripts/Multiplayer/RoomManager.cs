using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing.Scened;
using System.Collections.Generic;

namespace KanairoCity.Multiplayer
{
    /// <summary>
    /// Manages multiplayer room instances within the current scene environment.
    /// Handles room creation and joining for networked players.
    /// </summary>
    public class RoomManager : NetworkBehaviour
    {
        public static RoomManager Instance { get; private set; }

        private const string PRIMARY_SCENE_NAME = "GameScene";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Request to create a new room from the client.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ServerCreateRoom(string roomName, NetworkConnection conn = null)
        {
            if (conn == null) return;
            
            Debug.Log($"[RoomManager] Creating room: {roomName} for connection: {conn.ClientId}");
            
            // In a single-scene setup, we ensure the player is loaded into the GameScene
            SceneLoadData sld = new SceneLoadData(PRIMARY_SCENE_NAME);
            
            // We use global scene loading for now since we are staying in the current scene
            InstanceFinder.SceneManager.LoadConnectionScenes(conn, sld);
        }

        /// <summary>
        /// Request to join an existing session.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ServerJoinRoom(string roomName, NetworkConnection conn = null)
        {
            if (conn == null) return;

            Debug.Log($"[RoomManager] Connection {conn.ClientId} attempting to join room: {roomName}");
            
            // Since we are using the current scene, we simply ensure the client is loaded into it
            SceneLoadData sld = new SceneLoadData(PRIMARY_SCENE_NAME);
            InstanceFinder.SceneManager.LoadConnectionScenes(conn, sld);
        }
    }
}
