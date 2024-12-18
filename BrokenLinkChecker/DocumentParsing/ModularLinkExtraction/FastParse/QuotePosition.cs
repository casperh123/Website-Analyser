
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct QuotePosition 
{
    private const uint VALID_FLAG = 1u << 26;
    private const int LENGTH_SHIFT = 13;
    private const uint START_MASK = 0x1FFF;
    private const uint LENGTH_MASK = 0x1FFF << LENGTH_SHIFT;  // Pre-shifted mask
    
    private readonly uint _data;

    // Regular property access - still very fast due to JIT optimizations
    public int Start 
    { 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(_data & START_MASK);
    }

    public int Length 
    { 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)((_data & LENGTH_MASK) >> LENGTH_SHIFT); 
    }

    public bool IsValid 
    { 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_data & VALID_FLAG) == VALID_FLAG; 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QuotePosition(int start, int length, bool isValid)
    {
        _data = (uint)start & START_MASK |
                ((uint)length << LENGTH_SHIFT) & LENGTH_MASK |
                (isValid ? VALID_FLAG : 0);
    }
}