using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;
using UnityEngine.UI;

public class NexVRInteractable : VRInteractiveItem{

    private SelectionRadial selectionRadial;         // This controls when the selection is complete.
    private Button button;

    private void OnEnable()
    {
        button = GetComponent<Button>();
        selectionRadial = Camera.main.GetComponent<SelectionRadial>();

        OnOver += HandleOver;
        OnOut += HandleOut;
        OnClick += HandleOnClick;
    }


    private void OnDisable()
    {
        OnOver -= HandleOver;
        OnOut -= HandleOut;
        OnClick -= HandleOnClick;
    }

    private void HandleOver()
    {
        selectionRadial.Show();
    }

    private void HandleOut()
    {
        selectionRadial.Hide();
    }

    private void HandleOnClick()
    {
        button.onClick.Invoke();
    }
}
