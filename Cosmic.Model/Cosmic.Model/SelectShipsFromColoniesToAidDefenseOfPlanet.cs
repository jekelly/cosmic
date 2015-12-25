namespace Cosmic.Model
{
    class SelectShipsFromColoniesToAidDefenseOfPlanet : MoveShipsAction
    {
        public SelectShipsFromColoniesToAidDefenseOfPlanet(IPlayer player, IPlanet targetPlanet)
        {
            this.ActingPlayer = player;
            this.Min = 1;
            this.Max = 4;
            this.Source = state => state.GetShipsOnPlanets(player);
            this.Sink = (state, ship) => targetPlanet.AlliedDefenders.AddShip(ship);
        }
    }
}
