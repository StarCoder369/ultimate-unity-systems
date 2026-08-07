using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{
    [Header("Main")]
    // The transform of the cursor
    public RectTransform cursorTransform;
    public Canvas canvas;

    // The cursor image, can be a child or the same object as cursorTransform.
    public Image cursorImage;

    [Header("Cursor Smoothing")]
    public bool smoothing = false;
    public float smoothingSpeed = 20f;

    Sprite defaultSprite;

    private void Awake()
    {
        Cursor.visible = false;
    }

    void Start()
    {
        defaultSprite = cursorImage.sprite;
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Mouse.current.position.ReadValue(), canvas.worldCamera, out Vector2 localPosition);

        if (smoothing)
        {
            cursorTransform.localPosition = Vector3.Lerp(cursorTransform.localPosition, localPosition, smoothingSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            cursorTransform.localPosition = localPosition;
        }
    }

    public void SetCursor(Sprite sprite)
    {
        cursorImage.sprite = sprite;
    }

    public void SetCursorToDefault()
    {
        cursorImage.sprite = defaultSprite;
    }

    public void SetVisible(bool state)
    {
        cursorTransform.gameObject.SetActive(state);
    }

    public void SetScale(float scale)
    {
        cursorTransform.localScale = Vector3.one * scale;
    }

    public void SetRotation(float rotation)
    {
        cursorTransform.localRotation = Quaternion.Euler(0, 0, rotation);
    }
}