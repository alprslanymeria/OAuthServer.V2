namespace OAuthServer.V2.Core.Models;

public class UserPasskeyCredential
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public byte[] CredentialId { get; set; } = default!;
    public byte[] PublicKey { get; set; } = default!;
    public byte[] UserHandle { get; set; } = default!;
    public uint SignCount { get; set; }
    public string? AttestationFormat { get; set; }
    public Guid AaGuid { get; set; }
    public byte[]? AttestationObject { get; set; }
    public byte[]? AttestationClientDataJson { get; set; }
    public bool IsBackupEligible { get; set; }
    public bool IsBackedUp { get; set; }
    public string? Transports { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // NAVIGATION PROPERTY
    public User User { get; set; } = null!;
}
