using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCanvas : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    
    public void Show()
    {
        canvas.SetActive(true);
    }

    public void Hide() 
    {
        canvas.SetActive(false);
    }
}
