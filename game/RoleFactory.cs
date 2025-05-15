namespace ShipBattle.Roles
{
    public static class RoleFactory
    {
        public static IPlayerRole Create(PlayerRole role)
        {
            return role switch
            {
                PlayerRole.FastShip => new FastShipRole(),
                PlayerRole.Spy => new SpyRole(),
                PlayerRole.Medic => new MedicRole(),
                // ...
                _ => new DefaultAdmiralRole()
            };
        }
    }

}
