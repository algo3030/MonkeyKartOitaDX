using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MonkeyKart.Common;
using Unity.Netcode;
using UnityEngine;

public class ItemBox : NetworkBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    NetworkVariable<bool> isDisable;

    float rotationSpeed = 50f;
    
    void Awake()
    {
        isDisable = new();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isDisable.OnValueChanged += OnDisableStateChanged;
    }

    void OnDestroy()
    {
        isDisable.OnValueChanged -= OnDisableStateChanged;
    }

    void Update()
    {
        transform.Rotate(rotationSpeed* Time.deltaTime, rotationSpeed * Time.deltaTime, rotationSpeed * Time.deltaTime);
    }

    void OnDisableStateChanged(bool _,bool disable)
    {
        Log.d("LOG", $"Changed to {disable}");
        meshRenderer.enabled = !disable;
        if (IsServer && disable) ServerWaitAndRespawn().Forget();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDisable.Value) return;
        if (other.CompareTag(MonkeyKartTags.Player))
        {
            PickUpItemBoxServerRpc();
            ItemManager.I.GetItem();
            meshRenderer.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PickUpItemBoxServerRpc()
    {
        isDisable.Value = true;
    }

    async UniTask ServerWaitAndRespawn()
    {
        await UniTask.Delay(5000);
        isDisable.Value = false;
    }
}
