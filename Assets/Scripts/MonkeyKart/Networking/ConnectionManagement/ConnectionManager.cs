using Cysharp.Threading.Tasks;
using MonkeyKart.UnityService.Lobbies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading;
using MonkeyKart.Common;
using UniRx;
using Unity.Services.Relay;
using System;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using MonkeyKart.SceneManagement;
using VContainer;
using MonkeyKart.Networking.Session;
using static UnityEngine.UI.GridLayoutGroup;

namespace MonkeyKart.Networking.ConnectionManagement
{
    /// <summary>
    /// ConnectionState�̃X�e�[�g�}�V��
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {
        const string TAG = "ConnectionManager";
        internal const int MAX_PLAYERS = 6;
        internal const string CONNECTION_TYPE = "dtls";

        [Inject] internal NetworkManager networkManager;
        [Inject] internal SceneLoader sceneLoader;

        ReactiveProperty<ConnectionState> currentState = new();
        public IReadOnlyReactiveProperty<ConnectionState> CurrentState => currentState;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        // �F�؂𑖂点��
        public async UniTask<Result<Unit, string>> Initialize()
        {
            var authRes = await AuthAPI.InitializeAndSignInAsync();
            return authRes
                .OnSuccess(_ =>
                    {
                        Log.d(TAG, "Signed in successflly.");
                        Log.d(TAG, $"PlayerId: {AuthAPI.PlayerId}");
                        currentState.Value = new OfflineState(this, DisconnectReason.Init);
                        currentState.Value.Enter();
                    });
        }

        void Start()
        {
            networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            networkManager.OnServerStarted += OnServerStarted;
            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnTransportFailure += OnTransportFailure;
            networkManager.OnServerStopped += OnServerStopped;
        }

        private void OnDestroy()
        {
            networkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            networkManager.OnServerStarted -= OnServerStarted;
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            networkManager.OnTransportFailure -= OnTransportFailure;
            networkManager.OnServerStopped -= OnServerStopped;
        }

        internal void ChangeState(ConnectionState next)
        {
            if(!currentState.Value.CanTransitionTo(next)) 
            {
                throw new InvalidOperationException($"Cannot transition from {currentState.Value.GetType().Name} to {next.GetType().Name}.");
            }
            Log.d(TAG, $"ConnectionState changing. {currentState.Value.GetType().Name} ------> {next.GetType().Name}");
            currentState.Value?.Exit();
            currentState.Value = next;
            currentState.Value.Enter();
        }

        public void StartHost(string playerName, string lobbyName)
        {
            ChangeState(new StartingHostState(this, playerName: playerName, lobbyName: lobbyName));
        }

        public void JoinLobby(string playerName, string lobbyCode)
        {
            ChangeState(new ClientConnectingState(this, playerName, lobbyCode));
        }
        
        public Result<Unit, string> RequestShutdown()
        {
            if (currentState.Value is not OnlineState state) return "Connection state is not Online.";
            state.OnUserRequestedShutDown(); 
            return Unit.Default;
        }

        public SessionManager GetSessionManager()
        {
            if (currentState.Value is not HostingState state) throw new InvalidOperationException();
            return state.SessionManager;
        }

        // NetworkManager callbacks
        void OnClientDisconnectCallback(ulong clientId)
        {
            currentState.Value.OnClientDisconnected(clientId);
        }

        void OnClientConnectedCallback(ulong clientId)
        {
            currentState.Value.OnClientConnected(clientId);
        }

        void OnServerStarted()
        {
            currentState.Value.OnServerStarted();
        }

        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            currentState.Value.ApprovalCheck(request, response);
        }

        void OnTransportFailure()
        {
            if (currentState.Value is not OnlineState state)
            {
                Log.w(TAG, "Current state is offline, but OnTransportFailure sended.");
                return;
            }
            state.OnTransportFailure();
        }

        void OnServerStopped(bool _)
        {
            currentState.Value.OnServerStopped();
        }
    }
}