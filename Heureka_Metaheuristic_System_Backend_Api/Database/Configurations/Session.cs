using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("sessions");

            builder.HasIndex(e => e.StateId, "sessions_ibfk_1");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11) unsigned");

            builder.Property(e => e.StateId)
                .HasColumnName("state_id")
                .HasColumnType("int(11) unsigned");

            builder.HasOne(d => d.State)
                .WithMany(p => p.Sessions)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("sessions_ibfk_1");
        }
    }
}
