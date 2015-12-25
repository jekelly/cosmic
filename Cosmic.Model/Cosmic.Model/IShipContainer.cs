﻿using System.Collections.Generic;

namespace Cosmic.Model
{
    public interface IShipContainer
    {
        void AddShip(IShip ship);
        IEnumerable<IShip> GetShips();
        IEnumerable<IShip> GetShips(IPlayer player);
        void RemoveShip(IShip player);
    }

    public interface IHyperspaceGate : IShipContainer
    {
        IPlanet TargetPlanet { get; set; }
    }

    public interface IWarp : IShipContainer
    {
    }

    public interface IPlanet : IShipContainer
    {
        IShipContainer AlliedDefenders { get; }
        IPlayer Owner { get; }
    }
}
