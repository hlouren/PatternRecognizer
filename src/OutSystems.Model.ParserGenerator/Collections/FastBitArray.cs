using System.Collections.Generic;

namespace OutSystems.Model.ParserGenerator.Collections;

internal class FastBitArray {

    private const int BitsPerEntry = 64;
    private const int Shift = 6;
    private const int Mask = 64 - 1;

    private readonly int bitCount;
    private readonly ulong[] vectors;

    public FastBitArray(int bitCount) {
        this.bitCount = bitCount;
        vectors = new ulong[CalculateVectorCount(bitCount)];
    }

    public FastBitArray(FastBitArray other) {
        bitCount = other.bitCount;
        vectors = (ulong[])other.vectors.Clone();
    }

    public FastBitArray(int bitCount, IEnumerable<bool> data) : this(bitCount) {
        int index = 0;
        foreach (var bit in data) {
            this[index++] = bit;
        }
    }

    public FastBitArray(bool[] data) : this(data.Length) {
        for (int i = 0; i < data.Length; i++) {
            this[i] = data[i];
        }
    }

    internal static int CalculateVectorCount(int bitCount) =>
        bitCount == 0 ? 0 : 1 + (bitCount - 1) / BitsPerEntry;

    public bool this[int index] {
        get => (vectors[index >> Shift] & 1ul << (index & Mask)) != 0;
        set {
            if (value) {
                vectors[index >> Shift] |= 1ul << (index & Mask);
            } else {
                vectors[index >> Shift] &= ~(1ul << (index & Mask));
            }
        }
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(this, obj)) {
            return true;
        }

        if (obj is not FastBitArray other || vectors.Length != other.vectors.Length) {
            return false;
        }

        for (int i = 0; i < vectors.Length; i++) {
            if (vectors[i] != other.vectors[i]) {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode() {
        var result = 0ul;
        for (int i = 0; i < vectors.Length; i++) {
            result ^= vectors[i];
        }

        return (int)result;
    }
}
