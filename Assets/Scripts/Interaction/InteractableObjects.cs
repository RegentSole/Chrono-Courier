using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Interactable Settings")]
    [SerializeField] protected bool canBeActivatedByPlayer = true;
    [SerializeField] protected bool canBeActivatedByGhost = true;
    [SerializeField] protected float activationDelay = 0f;
    
    [Header("Visual Feedback")]
    [SerializeField] protected Color inactiveColor = Color.gray;
    [SerializeField] protected Color activeColor = Color.green;
    [SerializeField] protected SpriteRenderer indicatorSprite;
    
    protected bool isActivated = false;
    protected SpriteRenderer spriteRenderer;
    
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (indicatorSprite != null)
        {
            indicatorSprite.color = inactiveColor;
        }
        
        // Изначально неактивен
        SetVisualState(false);
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canBeActivatedByPlayer && !isActivated)
        {
            Activate();
        }
        else if (collision.CompareTag("Ghost") && canBeActivatedByGhost && !isActivated)
        {
            Activate();
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canBeActivatedByPlayer && isActivated)
        {
            Deactivate();
        }
        else if (collision.CompareTag("Ghost") && canBeActivatedByGhost && isActivated)
        {
            Deactivate();
        }
    }
    
    public virtual void Activate()
    {
        if (isActivated) return;
        
        if (activationDelay > 0)
        {
            StartCoroutine(ActivateWithDelay());
        }
        else
        {
            SetActivated(true);
        }
    }
    
    public virtual void Deactivate()
    {
        if (!isActivated) return;
        SetActivated(false);
    }
    
    private System.Collections.IEnumerator ActivateWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        SetActivated(true);
    }
    
    protected virtual void SetActivated(bool activated)
    {
        isActivated = activated;
        SetVisualState(activated);
        
        Debug.Log($"{gameObject.name} is now {(activated ? "activated" : "deactivated")}");
    }
    
    protected virtual void SetVisualState(bool activated)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = activated ? activeColor : inactiveColor;
        }
        
        if (indicatorSprite != null)
        {
            indicatorSprite.color = activated ? activeColor : inactiveColor;
        }
    }
}