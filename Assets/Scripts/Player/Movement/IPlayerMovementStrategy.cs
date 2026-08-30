public interface IPlayerMovementStrategy
{
    void Enter();
    void Exit();
    void Tick(float deltaTime);
    void Reset();
    void CancelActiveAction();
}
