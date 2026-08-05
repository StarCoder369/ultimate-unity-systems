using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderValueForwarder : MonoBehaviour
{
    public enum ValueType
    {
        Int,
        Float
    }
    public bool forwardValueOnStart;

    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }

    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Header("Output Type")]
    public ValueType valueType = ValueType.Int;

    [Header("Events")]
    public IntEvent OnIntValueChanged;
    public FloatEvent OnFloatValueChanged;

    private Slider slider;

    void Start()
    {
        ForwardValue();
    }

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void ForwardValue()
    {
        Debug.Log("Forwarding");
        switch (valueType)
        {
            case ValueType.Int:
                Debug.Log(slider.value);
                OnIntValueChanged?.Invoke(Mathf.RoundToInt(slider.value));
                break;

            case ValueType.Float:
                OnFloatValueChanged?.Invoke(slider.value);
                break;
        }
    }
}