using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmic.Model
{
    public class DefenderRewards
    {
        private readonly int count;

        public DefenderRewards(int count)
        {
            this.count = count;
        }

        public bool IsValid
        {
            get { return this.Ships + this.Cards == this.count; }
        }

        public int Count { get { return this.count; } }
        public int Ships { get; set; }
        public int Cards { get; set; }
    }

    public class DefaultResolutionPhase
    {
        public void Do(GameState game)
        {
            foreach (var player in game.PlayersInResolutionOrder)
            {
                var shipsAndSource = FindShipsInEncounter(player, game).ToArray();
                int count = shipsAndSource.Length;
                EncounterOutcome outcome = game.GetEncounterOutcome(player);
                if (outcome.HasFlag(EncounterOutcome.AllShipsToWarp))
                {
                    foreach (var ship in shipsAndSource)
                    {
                        game.MoveShipToWarp(ship.Source, ship.Ship);
                    }
                }
                if (outcome.HasFlag(EncounterOutcome.CollectCompensation))
                {
                    // TODO: better way to determine this?
                    var opponent = (player == game.ActivePlayer ? game.DefensePlayer : game.ActivePlayer);
                    game.TakeCompensation(player, opponent, count);
                }
                if (outcome.HasFlag(EncounterOutcome.DefenderRewards))
                {
                    DefenderRewards rewards = new DefenderRewards(count);
                    while (!rewards.IsValid)
                    {
                        player.ChooseRewards(rewards);
                    }
                    for (int i = 0; i < rewards.Cards; i++)
                    {
                        game.DrawCardToHand(player);
                    }
                    for (int i = 0; i < rewards.Ships; i++)
                    {
                        var ships = game.Warp.GetShips(player);
                        var ship = player.ChooseShip(ships);
                        var colonies = game.GetPlanetsWithColony(player);
                        var target = player.ChooseColony(colonies);
                        game.Warp.RemoveShip(ship);
                        target.AddShip(ship);
                    }
                }
                if (outcome.HasFlag(EncounterOutcome.EstablishColony))
                {
                    var targetPlanet = game.HyperspaceGate.TargetPlanet;
                    foreach (var ship in shipsAndSource)
                    {
                        if (!ship.Source.RemoveShip(ship.Ship))
                        {
                            throw new InvalidOperationException();
                        }
                        targetPlanet.AddShip(ship.Ship);
                    }
                }
            }
            if (game.GetEncounterResult() != EncounterResult.Success ||
                game.EncounterNumber != 0 ||
                !game.ActivePlayer.AcceptAnotherEncounter())
            {
                game.SetNextPlayer();
                game.StartNewTurn();
            }
            else
            {
                game.StartNewEncounter();
            }
        }

        private IEnumerable<ShipAndSource> FindShipsInEncounter(IPlayer player, GameState game)
        {
            var hyperspaceGate = game.HyperspaceGate;
            var targetPlanet = hyperspaceGate.TargetPlanet;

            var ships = GetShipAndSource(hyperspaceGate, player);
            if (player == game.DefensePlayer)
            {
                ships = ships.Concat(GetShipAndSource(targetPlanet, player));
            }
            ships = ships.Concat(GetShipAndSource(targetPlanet.AlliedDefenders, player));
            return ships;
        }

        private IEnumerable<ShipAndSource> GetShipAndSource(IShipContainer source, IPlayer player)
        {
            return source.GetShips(player).Select(ship => new ShipAndSource() { Ship = ship, Source = source });
        }
    }

    class ShipAndSource
    {
        public IShip Ship { get; set; }
        public IShipContainer Source { get; set; }
    }
}
