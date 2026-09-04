using Unity.Netcode.Components;

public class NetworkOwnerAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}