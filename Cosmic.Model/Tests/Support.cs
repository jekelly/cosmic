using System;
using System.Collections.Generic;
using System.Linq;
using Cosmic.Model;

namespace Tests
{
    class TestShip : IShip
    {
        public IPlayer Owner { get; set; }
    }

    class ChooserPlayer : NullPlayer
    {
        public Func<IPlanet> PlanetChooser { get; set; }
        public Func<ChooserPlayer> PlayerChooser { get; set; }
        public Func<IShip> ShipChooser { get; set; }
        public Func<IEnumerable<IPlayer>, IEnumerable<IPlayer>> OffensiveAllyInviteChooser { get; set; }
        public Func<IEnumerable<IPlayer>, IEnumerable<IPlayer>> DefensiveAllyInviteChooser { get; set; }
        public Func<Alliance> AllianceChooser { get; set; }

        public override IEnumerable<IPlayer> InviteOffensiveAllies(IEnumerable<IPlayer> invited)
        {
            return this.OffensiveAllyInviteChooser(invited);
        }

        public override IEnumerable<IPlayer> InviteDefensiveAllies(IEnumerable<IPlayer> invited)
        {
            return this.DefensiveAllyInviteChooser(invited);
        }

        public override Alliance ChooseAllianceSide(IEnumerable<Alliance> choices)
        {
            return this.AllianceChooser != null ? this.AllianceChooser() : Alliance.Neither;
        }

        public override IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players)
        {
            return this.PlayerChooser();
        }

        public override IShip ChooseShip(IEnumerable<IShip> ships)
        {
            return this.ShipChooser();
        }

        public override IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets)
        {
            return this.PlanetChooser();
        }

        public override IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets)
        {
            return this.PlanetChooser();
        }
    }

    class NullPlayer : IPlayer
    {
        public virtual bool AcceptEncounterInHomeSystem()
        {
            return false;
        }

        public virtual Alliance ChooseAllianceSide(IEnumerable<Alliance> choices)
        {
            return Alliance.Neither;
        }

        public virtual IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players)
        {
            return null;
        }

        public virtual IShip ChooseShip(IEnumerable<IShip> ships)
        {
            return null;
        }

        public virtual IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets)
        {
            return null;
        }

        public virtual IEnumerable<IPlayer> InviteDefensiveAllies(IEnumerable<IPlayer> invited)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IPlayer> InviteOffensiveAllies(IEnumerable<IPlayer> invited)
        {
            throw new NotImplementedException();
        }

        public virtual Alien SelectAlien(params Alien[] aliens)
        {
            return null;
        }

        public virtual IEncounterCard SelectEncounterCard()
        {
            return null;
        }

        public virtual IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets)
        {
            return null;
        }
    }

    class TestPlayer : IPlayer
    {
        public Alien SelectAlien(params Alien[] aliens)
        {
            return aliens[0];
        }

        public IPlanet PreferredPlanet { get; set; }
        public IShip PreferredShip { get; set; }

        public bool AcceptHomeSystemEncounters { get; set; }

        public IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets)
        {
            return this.PreferredPlanet ?? planets.First();
        }

        public IEncounterCard SelectEncounterCard()
        {
            throw new NotImplementedException();
        }

        public bool AcceptEncounterInHomeSystem()
        {
            return this.AcceptHomeSystemEncounters;
        }

        public IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets)
        {
            return this.PreferredPlanet ?? planets.First();
        }

        public IShip ChooseShip(IEnumerable<IShip> ships)
        {
            return this.PreferredShip ?? ships.First();
        }

        public IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players)
        {
            throw new NotImplementedException();
        }

        public Alliance ChooseAllianceSide(IEnumerable<Alliance> choices)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPlayer> InviteOffensiveAllies(IEnumerable<IPlayer> invited)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPlayer> InviteDefensiveAllies(IEnumerable<IPlayer> invited)
        {
            throw new NotImplementedException();
        }
    }

    class TestRandom : IRandom
    {
        public int Next(int max)
        {
            return 0;
        }

        public int Next(int min, int max)
        {
            return max - 1;
        }

        public T PickOne<T>(T[] items)
        {
            return items[0];
        }
    }
}
