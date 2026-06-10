namespace WorldCup.Domain.Abstractions;

/// <summary>Hashes and verifies user passwords. Plaintext is never stored.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
