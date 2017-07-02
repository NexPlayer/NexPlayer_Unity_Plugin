using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using VRStandardAssets.Utils;

public class NexVRInteractableSeekBar : VRInteractiveItem
{
    private SelectionRadial selectionRadial;
    private Slider slider;
    private RaycastHit lastHit;
    private BoxCollider leftSideOfTheSeekBarCollider;
    private BoxCollider sliderCollider;
    private float originalWidth = -1;

    private void OnEnable()
    {
        slider = GetComponent<Slider>();
        selectionRadial = Camera.main.GetComponent<SelectionRadial>();

        OnOver += HandleOver;
        OnOut += HandleOut;
        OnClick += HandleOnClick;

        float newWidth = GetComponent<RectTransform>().rect.width;
        if (originalWidth == -1 || originalWidth > newWidth) originalWidth = newWidth;


        GetComponent<BoxCollider>().size = new Vector3(originalWidth, 20, 0.001f);

        leftSideOfTheSeekBarCollider = transform.Find("LeftSideOfTheSeekBar").GetComponent<BoxCollider>();
        sliderCollider = slider.GetComponent<BoxCollider>();
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

    public void SetLastHit(RaycastHit hit)
    {
        lastHit = hit;
    }

    private void HandleOnClick()
    {
        // This is correct
        float widthInSeekBarInWoldsSpace = Vector3.Distance(sliderCollider.bounds.min, sliderCollider.bounds.max);
        float distanceFromHitPointToCenter = Vector3.Distance(leftSideOfTheSeekBarCollider.bounds.center, lastHit.point);

        slider.value = distanceFromHitPointToCenter / widthInSeekBarInWoldsSpace;
    }
}
