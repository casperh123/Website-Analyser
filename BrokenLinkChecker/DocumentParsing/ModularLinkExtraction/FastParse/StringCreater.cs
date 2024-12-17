using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public unsafe class StringCreater
{

    public static unsafe void ConvertUrlsToStrings(
        byte[] urlBuffer,
        int[] urlLengths,
        int count,
        int maxUrlLength,
        List<string> output)
    {
        fixed (byte* urlBufferPtr = urlBuffer)
        {
            for (int i = 0; i < count; i++)
            {
                int length = urlLengths[i];
                if (length > 0 && length < maxUrlLength)
                {
                    // Create string directly from byte pointer
                    string url = string.Create(length, (nint)(urlBufferPtr + i * maxUrlLength), (chars, ptr) =>
                    {
                        // Direct pointer copy without Span overhead
                        var src = (byte*)ptr;
                        fixed (char* dest = chars)
                        {
                            int length = chars.Length;
                            int i = 0;
                            int limit = length - (length % 8); // Largest multiple of 8 less than or equal to length

// Process 8 elements at a time
                            for (; i < limit; i += 8)
                            {
                                dest[i]     = (char)src[i];
                                dest[i + 1] = (char)src[i + 1];
                                dest[i + 2] = (char)src[i + 2];
                                dest[i + 3] = (char)src[i + 3];
                                dest[i + 4] = (char)src[i + 4];
                                dest[i + 5] = (char)src[i + 5];
                                dest[i + 6] = (char)src[i + 6];
                                dest[i + 7] = (char)src[i + 7];
                            }

// Handle any remaining elements
                            for (; i < length; i++)
                            {
                                dest[i] = (char)src[i];
                            }
                        }
                    });

                    output.Add(url);
                }
            }
        }
    }
}