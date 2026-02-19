using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OAuthServer.V2.Core.Models;

namespace OAuthServer.V2.Data.Configurations;

public class UserPasskeyCredentialConfiguration : IEntityTypeConfiguration<UserPasskeyCredential>
{
    public void Configure(EntityTypeBuilder<UserPasskeyCredential> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CredentialId).IsRequired();
        builder.Property(x => x.PublicKey).IsRequired();
        builder.Property(x => x.UserHandle).IsRequired();
        builder.Property(x => x.UserId).IsRequired();

        builder.HasIndex(x => x.CredentialId);
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
