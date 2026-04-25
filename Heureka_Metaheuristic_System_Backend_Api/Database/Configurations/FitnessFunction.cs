using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Configurations
{
    public class FitnessFunctionConfiguration : IEntityTypeConfiguration<FitnessFunction>
    {
        public void Configure(EntityTypeBuilder<FitnessFunction> builder)
        {
            builder.ToTable("fitness_functions");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11) unsigned");

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("varchar(64)")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(e => e.ClassName)
                .HasColumnName("class_name")
                .HasColumnType("varchar(64)")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(e => e.FileName)
                .HasColumnName("file_name")
                .HasColumnType("varchar(64)")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(e => e.IsRemoveable)
                .HasColumnName("is_removeable")
                .HasColumnType("tinyint(1) unsigned");

            builder.Property(e => e.Dimension)
                .HasColumnName("dimension")
                .HasColumnType("tinyint(4) unsigned");

            builder.Property(e => e.Domain)
                .HasColumnName("domain")
                .HasColumnType("text")
                .HasDefaultValue("{}");
        }
    }
}
