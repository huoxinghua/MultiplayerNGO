namespace _Project.Code.Gameplay.Player.UsableItems
{
    public interface IHeldItem
    {
        public void Use();
        public void Drop();
        public void Pickup();
        public void SwapOff();
        public void SwapTo();
    }
}
