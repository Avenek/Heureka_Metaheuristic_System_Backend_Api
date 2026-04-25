using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Configurations
{
    public class SessionStateConfiguration : IEntityTypeConfiguration<SessionState>
    {
        public void Configure(EntityTypeBuilder<SessionState> builder)
        {
            builder.ToTable("session_states");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11) unsigned");

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("tinytext")
                .IsRequired();
        }
    }
}
