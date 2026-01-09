/// <summary>
/// Interface for objects that can be interacted with by players.
/// Implement this on resource nodes, items, doors, NPCs, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Text shown in the interaction prompt (e.g., "Gather Wood")
    /// </summary>
    string InteractionPrompt { get; }
    
    /// <summary>
    /// Whether this object can currently be interacted with
    /// </summary>
    bool CanInteract { get; }
    
    /// <summary>
    /// Time required to hold E to complete interaction (0 = instant)
    /// </summary>
    float InteractionTime { get; }
    
    /// <summary>
    /// Called when player interacts with this object
    /// </summary>
    void Interact(PlayerInteraction player);
}
