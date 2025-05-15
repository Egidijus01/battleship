using System;

namespace ShipBattle.Exceptions
{
    public class EgidijusGagelaException : Exception
    {
        public DateTime ErrorTime { get; }
        public string AdditionalInfo { get; }

        public EgidijusGagelaException(string message) : base(message)
        {
            ErrorTime = DateTime.Now;
        }

        public EgidijusGagelaException(string message, string additionalInfo) : base(message)
        {
            ErrorTime = DateTime.Now;
            AdditionalInfo = additionalInfo;
        }

        public override string ToString()
        {
            return $"{base.ToString()}\nĮvyko: {ErrorTime}\nPapildoma info: {AdditionalInfo}";
        }
    }
}
