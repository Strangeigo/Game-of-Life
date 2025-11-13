using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool isActive = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    public void ToggleState()
    {
        isActive = !isActive;
        UpdateColor();
    }

    public void SetState(bool state)
    {
        isActive = state;
        UpdateColor();
    }

    private void UpdateColor()
    {
        spriteRenderer.color = isActive ? Color.white : Color.black;
    }
}
