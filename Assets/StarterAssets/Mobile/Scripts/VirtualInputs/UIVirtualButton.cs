using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    [System.Serializable]
    public class Event : UnityEvent { }

    [Header("Output")]
    public BoolEvent buttonStateOutputEvent;
    public Event buttonClickOutputEvent;
    
    [Header("Settings")]
    public float clickDelay = 0.0f; // Delay before firing click event

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickDelay > 0f)
        {
            Invoke(nameof(OutputButtonClickEvent), clickDelay);
        }
        else
        {
            OutputButtonClickEvent();
        }
        OutputButtonStateValue(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OutputButtonStateValue(false);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // if (clickDelay > 0f)
        // {
        //     Invoke(nameof(OutputButtonClickEvent), clickDelay);
        // }
        // else
        // {
        //     OutputButtonClickEvent();
        // }
    }

    void OutputButtonStateValue(bool buttonState)
    {
        buttonStateOutputEvent.Invoke(buttonState);
    }

    void OutputButtonClickEvent()
    {
        buttonClickOutputEvent.Invoke();
    }

}
