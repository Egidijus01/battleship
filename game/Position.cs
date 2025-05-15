namespace ShipBattle.Core
{
    public class Position : IEquatable<Position>
    {
        public int Row { get; }
        public int Column { get; }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public static Position FromString(string coordinates)
        {
            if (string.IsNullOrEmpty(coordinates) || coordinates.Length < 2)
            {
                throw new FormatException("Neteisingas koordinačių formatas. Tinkamas formatas: A1, B5, t.t.");
            }

            char columnChar = coordinates[0];
            if (!char.IsLetter(columnChar))
            {
                throw new FormatException("Pirmasis simbolis turėtų būti raidė, žyminti stulpelį.");
            }

            string rowString = coordinates.Substring(1);
            if (!int.TryParse(rowString, out int row) || row < 1)
            {
                throw new FormatException("Po raidės turėtų būti skaičius, žymintis eilutę.");
            }

            int column = char.ToUpper(columnChar) - 'A';

            return new Position(row - 1, column);
        }
        public override string ToString()
        {
            char columnChar = (char)('A' + Column);
            return $"{columnChar}{Row + 1}";
        }

        public override bool Equals(object obj)
        {
            return obj is Position position && Equals(position);
        }

        public bool Equals(Position other)
        {
            return other != null && Row == other.Row && Column == other.Column;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(Position left, Position right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }
    }
}
