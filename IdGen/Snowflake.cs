namespace IdGen
{
    public class Snowflake : IIdGen<long>
    {
        private const long StartStamp = 1480166465631L;

        private const int SequenceBit = 12;
        private const int WorkerBit = 5;
        private const int DatacenterBit = 5;

        private const long MaxDatacenterNum = -1L ^ (-1L << DatacenterBit);
        private const long MaxWorkerNum = -1L ^ (-1L << WorkerBit);
        private const long MaxSequence = -1L ^ (-1L << SequenceBit);

        private const int WorkerLeft = SequenceBit;
        private const int DatacenterLeft = SequenceBit + WorkerBit;
        private const int TimestampLeft = DatacenterLeft + DatacenterBit;

        private readonly long _datacenterId;
        private readonly long _workerId;
        private long _sequence;
        private long _lastStamp = -1L;
        
        public Snowflake() { }
        public Snowflake(long cId, long wId)
        {
            if (cId is > MaxDatacenterNum or < 0) 
                throw new ArgumentException($"center id should in [0, {MaxDatacenterNum}]");
            if (wId is > MaxWorkerNum or < 0) 
                throw new ArgumentException($"worker id should in [0, {MaxWorkerNum}]");
            _datacenterId = cId;
            _workerId = wId;
        }
        
        public long Gen()
        {
            var currStamp = GetNewStamp();
            if (currStamp < _lastStamp) 
                throw new InvalidDataException("Time reversal, failure to generate id");

            if (currStamp == _lastStamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0L) currStamp = GetNextMill();
            }
            else _sequence = 0L;
            _lastStamp = currStamp;

            return (currStamp - StartStamp) << TimestampLeft
                          | _datacenterId << DatacenterLeft
                          | _workerId << WorkerLeft
                          | _sequence;
        }

        private long GetNextMill()
        {
            long mill = GetNewStamp();
            while (mill <= _lastStamp)
            {
                mill = GetNewStamp();
            }
            return mill;
        }

        private static long GetNewStamp()
        {
            return DateTimeOffset.UtcNow.Millisecond;
        }
    }
}
