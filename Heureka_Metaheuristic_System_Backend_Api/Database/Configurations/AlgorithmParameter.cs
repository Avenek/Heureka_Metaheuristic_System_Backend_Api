using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Configurations
{
    public class AlgorithmParameterConfiguration : IEntityTypeConfiguration<AlgorithmParameter>
    {
        public void Configure(EntityTypeBuilder<AlgorithmParameter> builder)
        {
            builder.ToTable("algorithm_parameters");

            builder.HasIndex(e => e.AlgorithmId, "algorithm_parameters_ibfk_1");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("varchar(64)")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(e => e.AlgorithmId)
                .HasColumnName("algorithm_id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.MinValue)
                .HasColumnName("min_value")
                .HasColumnType("float");

            builder.Property(e => e.MaxValue)
                .HasColumnName("max_value")
                .HasColumnType("float");

            builder.HasOne(d => d.Algorithm)
                .WithMany(p => p.Parameters)
                .HasForeignKey(d => d.AlgorithmId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("algorithm_parameters_ibfk_1");
        }
    }
}
