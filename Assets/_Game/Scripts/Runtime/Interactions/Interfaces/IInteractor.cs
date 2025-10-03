namespace Game.Runtime.Interactions.Interfaces
{
    public interface IInteractor
    {
        bool IsInteracting { get; }
        bool CanStartInteraction { get; }
        void StartInteraction(IInteractable interactable);
        void EndInteraction(IInteractable interactable);
    }
}