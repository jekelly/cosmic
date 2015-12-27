using System.Collections.Generic;

namespace Cosmic.Model
{
    public interface IPlayer
    {
        Alien SelectAlien(params Alien[] aliens);
        IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets);
        IEncounterCard SelectEncounterCard();
        bool AcceptEncounterInHomeSystem();
        IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets);
        IShip ChooseShip(IEnumerable<IShip> ships);
        IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players);
        Alliance ChooseAllianceSide(IEnumerable<Alliance> choices);
        IEnumerable<IPlayer> InviteOffensiveAllies(IEnumerable<IPlayer> invited);
        IEnumerable<IPlayer> InviteDefensiveAllies(IEnumerable<IPlayer> invited);
        IEncounterCard ChooseEncounterCard(IEnumerable<IEncounterCard> hand);
    }
}
