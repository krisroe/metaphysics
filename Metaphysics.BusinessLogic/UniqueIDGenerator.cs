namespace Metaphysics.BusinessLogic;

public class UniqueIDGenerator
{
    private static readonly Guid MaxGuid = new("ffffffff-ffff-ffff-ffff-ffffffffffff");

    private Guid _currentID = Guid.Empty;
    private readonly object _lock = new();

    public Guid GenerateID()
    {
        lock (_lock)
        {
            if (_currentID == MaxGuid)
                throw new InvalidOperationException("Cannot increment beyond the maximum Guid value.");

            Span<byte> bytes = stackalloc byte[16];
            _currentID.TryWriteBytes(bytes, bigEndian: true, out _);

            for (int i = 15; i >= 0; i--)
            {
                if (bytes[i] < 0xFF)
                {
                    bytes[i]++;
                    break;
                }
                bytes[i] = 0;
            }

            _currentID = new Guid(bytes, bigEndian: true);
            return _currentID;
        }
    }
}
