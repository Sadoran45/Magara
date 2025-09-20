/// <summary>
/// Interface for components that want to listen to animation events.
/// Implement this interface to receive callbacks when specific AnimEventSO events are triggered.
/// </summary>
public interface IAnimationEventListener
{
    /// <summary>
    /// Called when an animation event is triggered.
    /// </summary>
    /// <param name="animEvent">The AnimEventSO that was triggered</param>
    void OnAnimationEvent(AnimEventSO animEvent);
}