namespace ShipBattle.Roles
{
    public class FastShipRole : IPlayerRole
    {
        public string Name => "FastShip";

        public void UseAbility(Iplayer current, Iplayer opponent, GameManager manager)
        {
            manager.EnableDoubleShot();
            console.WriteLine("FasHip aktyvuotas: gali sauti du kartus.")
        }
    }
}
