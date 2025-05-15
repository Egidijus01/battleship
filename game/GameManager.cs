using System;
using ShipBattle.Roles;
using System.Collections.Generic;

namespace ShipBattle
{
    public class GameManager
    {
        private IRules _rules;
        private ILogger _logger;
        private Player _player1;
        private Player _player2;
        private Player _currentPlayer;
        private Player _opponentPlayer;
        private bool _gameOver;

        private Stack<MoveRecord> _moveHistory = new Stack<MoveRecord>();

        private bool _doubleShotAllowed = false;
        private int _shotsThisTurn = 0;

        public static int TotalShots = 0;

        public GameManager()
        {
            _rules = new Rules();
            _logger = new Logger();
        }

        public void StartGame()
        {
            Console.Clear();
            _rules.SetupRules();

            Console.Clear();
            DisplayRulesSummary();

            _player1 = CreatePlayer("pirmojo");
            _player2 = CreatePlayer("antrojo");

            AssignRolesAndRoleInstances();

            Random rand = new Random();
            if (rand.Next(2) == 0)
            {
                _currentPlayer = _player1;
                _opponentPlayer = _player2;
            }
            else
            {
                _currentPlayer = _player2;
                _opponentPlayer = _player1;
            }

            Console.WriteLine($"Pirmas pradeda: {_currentPlayer.Name}");
            PlaceShipsForPlayers();

            _logger.LogGameStart(_player1, _player2, _rules);

            while (!_gameOver)
            {
                GameCycle();
            }
        }

        private void AssignRolesAndRoleInstances()
        {
            // Pvz. priskiriame role enum ir pagal tai sukurti RoleInstance
            _player1.Role = PlayerRole.Bomber;
            _player1.RoleInstance = RoleFactory.CreateRole(_player1.Role);

            _player2.Role = PlayerRole.Spy;
            _player2.RoleInstance = RoleFactory.CreateRole(_player2.Role);
        }

        private void DisplayRulesSummary()
        {
            Console.WriteLine($"Žaidimo lenta: {_rules.BoardSize}x{_rules.BoardSize}\nLaivų skaičius:");
            foreach (var ship in _rules.Ships) Console.WriteLine($"- {ship.Key} ilgio laivai: {ship.Value}");
            Console.WriteLine($"Bombų skaičius:\n- Radarai: {_rules.RadarBombs}\n- Sprogstamosios bangos: {_rules.ExplosiveBombs}\n");
        }

        private Player CreatePlayer(string playerNumber)
        {
            Console.WriteLine($"Įveskite {playerNumber} žaidėjo vardą:");
            string name = Console.ReadLine();
            return new Player(name, _rules.BoardSize, _rules.Ships, _rules.RadarBombs, _rules.ExplosiveBombs, PlayerRole.Admiral);
        }

        private void PlaceShipsForPlayers()
        {
            ExecuteShipPlacement(_player1);
            Console.Clear();
            ExecuteShipPlacement(_player2);
        }

        private void ExecuteShipPlacement(Player player)
        {
            Console.Clear();
            Console.WriteLine($"{player.Name}, išdėstykite savo laivus.\nGalimos kryptys: H (horizontaliai) arba V (vertikaliai)");

            foreach (var shipEntry in _rules.Ships)
            {
                for (int i = 0; i < shipEntry.Value; i++)
                {
                    PlaceSingleShip(player, shipEntry.Key, shipEntry.Value - i);
                }
            }

            Console.WriteLine($"{player.Name}, visi laivai išdėstyti!\nPaspauskite bet kurį klavišą...");
            Console.ReadKey();
        }

        private void PlaceSingleShip(Player player, int length, int remaining)
        {
            bool placed = false;
            while (!placed)
            {
                Console.Clear();
                player.PrintOwnBoard();
                Console.WriteLine($"Išdėstykite {length} ilgio laivą (liko: {remaining})");

                var (pos, dir) = GetShipPlacementDetails();
                try
                {
                    placed = player.TryPlaceShip(new Ship(length, pos, dir));
                    if (!placed) Console.WriteLine("Negalima pozicija. Bandykite dar kartą.");
                }
                catch (FormatException)
                {
                    HandleInvalidInput("Neteisingas koordinačių formatas");
                }
            }
        }

        private (Position pos, ShipDirection dir) GetShipPlacementDetails()
        {
            while (true)
            {
                try
                {
                    Console.Write("Įveskite koordinates (pvz., A1): ");
                    Position position = Position.FromString(Console.ReadLine().ToUpper());

                    Console.Write("Įveskite kryptį (H/V): ");
                    string direction = Console.ReadLine().ToUpper();
                    if (direction != "H" && direction != "V")
                        throw new FormatException();

                    return (position, direction == "H" ? ShipDirection.Horizontal : ShipDirection.Vertical);
                }
                catch (FormatException)
                {
                    HandleInvalidInput("Netinkama įvestis. Bandykite dar kartą.");
                }
            }
        }

        private void GameCycle()
        {
            Console.Clear();
            Console.WriteLine($"{_currentPlayer.Name} ėjimas (Rolė: {_currentPlayer.Role})\n");
            _currentPlayer.PrintOpponentBoard();
            _currentPlayer.PrintOwnBoard();
            Console.WriteLine($"\nRadarai: {_currentPlayer.RadarBombs}, Sprogstamosios bangos: {_currentPlayer.ExplosiveBombs}");

            _shotsThisTurn = 0;
            _doubleShotAllowed = false;

            bool maintainTurn = false;
            do
            {
                maintainTurn = ExecutePlayerAction();

                if (_opponentPlayer.AreAllShipsSunk())
                {
                    EndGame();
                    return;
                }

                _shotsThisTurn++;
                if (!_doubleShotAllowed || _shotsThisTurn >= 2)
                    break;

                Console.WriteLine("Galite atlikti dar vieną šūvį (dėl FastShip rolės).");
            }
            while (maintainTurn && _doubleShotAllowed && _shotsThisTurn < 2);

            if (!maintainTurn) SwitchPlayers();

            Console.WriteLine("Paspauskite bet kurį klavišą...");
            Console.ReadKey();
        }

        private bool ExecutePlayerAction()
        {
            while (true)
            {
                Console.WriteLine("Pasirinkite veiksmą:\n1. Šauti\n2. Naudoti specialų gebėjimą");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        return ExecutePositionAction(MakeShot);

                    case "2":
                        UseSpecialAbility();
                        return false;

                    default:
                        HandleInvalidInput("Neteisingas pasirinkimas");
                        break;
                }
            }
        }

        private bool ExecutePositionAction(Func<Position, bool> action)
        {
            while (true)
            {
                try
                {
                    Console.Write("Įveskite koordinates: ");
                    Position position = Position.FromString(Console.ReadLine().ToUpper());
                    return action(position);
                }
                catch (FormatException)
                {
                    HandleInvalidInput("Neteisingas koordinačių formatas");
                }
                catch (InvalidOperationException ex)
                {
                    HandleInvalidInput(ex.Message);
                }
            }
        }

        private bool MakeShot(Position position)
        {
            ShotResult result = _currentPlayer.Shoot(position, _opponentPlayer.OwnBoard);
            _logger.LogShot(_currentPlayer.Name, position.ToString(), result);
            TotalShots++;

            _moveHistory.Push(new MoveRecord(_currentPlayer, _opponentPlayer, position, result));

            Console.WriteLine(result switch
            {
                ShotResult.Hit => "Pataikėte!",
                ShotResult.Sunk => "Pataikėte ir nuskandinote laivą!",
                _ => "Nepataikėte!"
            });

            return result == ShotResult.Hit || result == ShotResult.Sunk;
        }

        private void UseSpecialAbility()
        {
            if (_currentPlayer == null) return;

            if (_currentPlayer.Role == PlayerRole.FastShip)
            {
                if (_doubleShotAllowed)
                {
                    Console.WriteLine("Specialus gebėjimas jau aktyvuotas šiam ėjimui.");
                    return;
                }
                _doubleShotAllowed = true;
                Console.WriteLine("Greitas laivas leidžia šauti du kartus šį ėjimą!");
                _currentPlayer.RoleInstance.UseAbility(_currentPlayer, _opponentPlayer, this);
                return;
            }

            Console.WriteLine("Pasirinkite specialų gebėjimą naudoti:");
            Console.WriteLine("1. Radaras");
            Console.WriteLine("2. Sprogstamoji bomba");
            Console.WriteLine("3. Šnipas (atveria eilutę arba stulpelį)");
            Console.WriteLine("4. Inžinierius (sutvirtina laivą)");
            Console.WriteLine("5. Taktikas (atšaukia paskutinį ėjimą)");
            Console.WriteLine("6. Medikas (pataiso laivą)");
            Console.Write("Jūsų pasirinkimas: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ExecutePositionAction(pos =>
                    {
                        _currentPlayer.UseRadar(pos, _opponentPlayer.OwnBoard);
                        _logger.LogRadarUse(_currentPlayer.Name, pos.ToString());
                        Console.WriteLine("Radaras panaudotas.");
                        return false;
                    });
                    break;

                case "2":
                    ExecutePositionAction(pos =>
                    {
                        bool hit = _currentPlayer.UseExplosiveBomb(pos, _opponentPlayer.OwnBoard);
                        _logger.LogExplosiveBombUse(_currentPlayer.Name, pos.ToString(), hit);
                        Console.WriteLine(hit ? "Pataikėte!" : "Nepataikėte!");
                        return hit;
                    });
                    break;

                case "3":
                    ExecutePositionAction(pos =>
                    {
                        _currentPlayer.RoleInstance.UseAbility(_currentPlayer, _opponentPlayer, this, pos);
                        Console.WriteLine("Šnipas panaudojo šnipinėjimo įgūdžius!");
                        return false;
                    });
                    break;

                case "4":
                    _currentPlayer.RoleInstance.UseAbility(_currentPlayer, _opponentPlayer, this);
                    Console.WriteLine("Inžinierius sutvirtino vieną laivą!");
                    break;

                case "5":
                    _currentPlayer.RoleInstance.UseAbility(_currentPlayer, _opponentPlayer, this);
                    break;

                case "6":
                    _currentPlayer.RoleInstance.UseAbility(_currentPlayer, _opponentPlayer, this);
                    Console.WriteLine("Medikas pataisė laivą!");
                    break;

                default:
                    Console.WriteLine("Neteisingas pasirinkimas.");
                    break;
            }
        }

        public void UndoLastMove(Player player)
        {
            if (_moveHistory.Count == 0)
            {
                Console.WriteLine("Nėra ėjimų, kuriuos būtų galima atšaukti.");
                return;
            }

            var lastMove = _moveHistory.Pop();

            if (lastMove.Shooter != player)
            {
                Console.WriteLine("Galite atšaukti tik savo paskutinį ėjimą.");
                _moveHistory.Push(lastMove);
                return;
            }

            lastMove.Target.OwnBoard.RevertShot(lastMove.ShotPosition, lastMove.Result);
            Console.WriteLine("Paskutinis ėjimas atšauktas.");

            // Jei reikia, pakeisti žaidėjo einamumą
            if (_currentPlayer != player)
                SwitchPlayers();
        }

        private void EndGame()
        {
            _gameOver = true;
            Console.Clear();
            Console.WriteLine($"Žaidimas baigtas! {_currentPlayer.Name} laimėjo!");
            Console.WriteLine($"Iš viso atlikta šūvių: {TotalShots}");
            _logger.LogGameEnd(_currentPlayer.Name);
            _logger.Dispose();
        }

        private void SwitchPlayers() => (_currentPlayer, _opponentPlayer) = (_opponentPlayer, _currentPlayer);

        private void HandleInvalidInput(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Paspauskite bet kurį klavišą...");
            Console.ReadKey();
        }

        private class MoveRecord
        {
            public Player Shooter { get; }
            public Player Target { get; }
            public Position ShotPosition { get; }
            public ShotResult Result { get; }

            public MoveRecord(Player shooter, Player target, Position position, ShotResult result)
            {
                Shooter = shooter;
                Target = target;
                ShotPosition = position;
                Result = result;
            }
        }
    }

    public static class RoleFactory
    {
        public static IPlayerRole CreateRole(PlayerRole role)
        {
            return role switch
            {
                PlayerRole.FastShip => new FastShipRole(),
                PlayerRole.Tactician => new TacticianRole(),
                PlayerRole.Bomber => new BomberRole(),
                PlayerRole.Spy => new SpyRole(),
                PlayerRole.Engineer => new EngineerRole(),
                PlayerRole.Medic => new MedicRole(),
                PlayerRole.Admiral => new AdmiralRole(),
                _ => throw new ArgumentException("Nežinoma rolė"),
            };
        }
    }
}
