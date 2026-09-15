namespace Game.Effects.Interfaces
{
    public interface IScreenEffects
    {
        public void Punch(float strength);
        public void Present(float intensity, float deltaTime);
        public void Reset();
    }
}
