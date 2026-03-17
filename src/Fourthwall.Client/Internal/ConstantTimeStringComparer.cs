using System.Security.Cryptography;
using System.Text;

namespace Fourthwall.Client.Internal;

/// <summary>
/// Provides a constant-time comparison helper for secrets and signatures.
/// </summary>
internal static class ConstantTimeStringComparer
{
    /// <summary>
    /// Compares two strings in constant time using their UTF-8 byte representation.
    /// </summary>
    /// <param name="left">The first string.</param>
    /// <param name="right">The second string.</param>
    /// <returns>
    /// <see langword="true"/> when both values are non-null and byte-for-byte equal;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool Equals(string? left, string? right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        byte[] leftBytes = Encoding.UTF8.GetBytes(left);
        byte[] rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}