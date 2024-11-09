using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MonkeyKart.GamePlaying.Item;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ItemDisplay : MonoBehaviour
{
    [SerializeField] Image image;
    
    void Start()
    {
        ItemManager.I.CurrentItem.Subscribe(OnItemChanged).AddTo(this);
    }

    void OnItemChanged(Item currentItem)
    {
        if (currentItem == null)
        {
            image.enabled = false;
            image.sprite = null;
            return;
        }
        image.enabled = true;
        image.sprite = currentItem.itemSprite;
    }
}
