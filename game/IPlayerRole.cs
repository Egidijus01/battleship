namespace ShipBattle.Core
{
    public interface IPlayerRole
    {
        string Name { get; }
        void UseAbility(Player current, Player opponent, GameManager manager);
    }

}
