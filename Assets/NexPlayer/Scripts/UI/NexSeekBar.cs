using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class NexSeekBar : MonoBehaviour
{
    [Tooltip("The graphic used for the sliding secondary “handle” part of the control")]
    public RectTransform handleRect;

    [Range(0.0f, 1.0f)]
    [Tooltip("Current numeric value of the secondary slider. If the value is set in the inspector it will be used as the initial value, but this will change at runtime when the value changes.")]
    public float secondaryValue;

    private Slider mainSlider;

    void Awake()
    {
        mainSlider = GetComponent<Slider>();
    }

    void Update ()
    {
        handleRect.anchorMax = new Vector2(secondaryValue, 1.0f);
    }

    /// <summary>
    /// Set the secondary value of the SeekBar. Can be used to represent the buffered time
    /// </summary>
    /// <param name="value">secondary value of the seekBar</param>
    public void SetSecondaryValue(float value)
    {
        secondaryValue = value;
    }

    /// <summary>
    /// Set the main value of the SeekBar. Can be used to represent the current time of the playback
    /// </summary>
    /// <param name="value">primary value of the seekBar</param>
    public void SetValue(float value)
    {
        // In case the component is disabled at the beginning
        if (mainSlider == null) mainSlider = GetComponent<Slider>();

        mainSlider.value = value;
    }

    /// <summary>
    /// Returns the main value of the seekBar
    /// </summary>
    /// <returns>The main value of the seekBar</returns>
    public float GetValue()
    {
        return mainSlider.value;
    }
} 
