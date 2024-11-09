using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MonkeyKart.Common;
using MonkeyKart.Common.UI.Button;
using MonkeyKart.GamePlaying;
using MonkeyKart.GamePlaying.Item;
using MonkeyKart.GamePlaying.Item.ItemSOs;
using UnityEngine;
using UniRx;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using Quaternion = System.Numerics.Quaternion;

public class ItemManager : NetworkBehaviour
{
    public static ItemManager I;

    [SerializeField] GameAudioManager gameAudioManager;
    [SerializeField] GameObject kabosuPfb;
    [SerializeField] GameObject hotSpringPfb;
    Rigidbody localPlayerRb;
    [SerializeField] SimpleButton itemButton;
    [SerializeField] List<Item> items;
    public ReactiveProperty<Item> CurrentItem = new();

    void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        itemButton.OnClickDown.Subscribe(_ => UseItem()).AddTo(this);
    }

    public void Init(Rigidbody localPlayerRb)
    {
        this.localPlayerRb = localPlayerRb;
    }

    public void GetItem()
    {
        gameAudioManager.MakeSE(gameAudioManager.BreakItemSE);
        if (CurrentItem.Value != null) return;
        var gotItem = items.RandomGet();
        gameAudioManager.MakeSE(gameAudioManager.GetItemSE);
        CurrentItem.Value = gotItem;
    }

    void UseItem()
    {
        Log.d("Item", "Used");
        if (CurrentItem.Value == null) return;
        
        switch (CurrentItem.Value)
        {
            case Kabosu kabosu:
                UseKabosu();
                break;
            case Siitake siitake:
                localPlayerRb.gameObject.GetComponent<PlayerMovement>().Dash();
                break;
            case HotSpring hotSpring:
                UseHotSpringServerRpc(localPlayerRb.position - localPlayerRb.transform.forward * 9f);
                break;
            default:
                break;
        }
        gameAudioManager.MakeSE(gameAudioManager.UseItemSE);
        CurrentItem.Value = null;
    }

    void UseKabosu()
    {
        UseKabosuServerRpc(localPlayerRb.transform.position,
            localPlayerRb.velocity * 2f + localPlayerRb.transform.forward * 30f);
    }


    [ServerRpc(RequireOwnership = false)]
    void UseKabosuServerRpc(Vector3 playerPos, Vector3 velocity, ServerRpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        var kabosuIns = Instantiate(kabosuPfb, playerPos + velocity.normalized * 6f + Vector3.up,
            quaternion.Euler(new Vector3(-90, 0, 0)));
        var kabosuNetObj = kabosuIns.GetComponent<NetworkObject>();
        kabosuNetObj.SpawnWithOwnership(senderId);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { senderId }
            }
        };
        InitKabosuClientRpc(velocity, kabosuNetObj.NetworkObjectId, clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    void UseHotSpringServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
    {
        var hotSpringIns = Instantiate(hotSpringPfb, position, UnityEngine.Quaternion.identity);
        var hotSpringNetObj = hotSpringIns.GetComponent<NetworkObject>();
        hotSpringNetObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    void InitKabosuClientRpc(Vector3 velocity, ulong networkObjId, ClientRpcParams rpcParams = default)
    {
        InitKabosu(velocity,networkObjId).Forget();
    }

    async UniTask InitKabosu(Vector3 velocity, ulong networkObjId)
    {
        var kabosuObj = NetworkManager.SpawnManager.SpawnedObjects[networkObjId];
        var kabosuItem = kabosuObj.GetComponent<KabosuItem>();
        kabosuItem.Init(velocity);
        kabosuItem.enabled = true;
    }
}