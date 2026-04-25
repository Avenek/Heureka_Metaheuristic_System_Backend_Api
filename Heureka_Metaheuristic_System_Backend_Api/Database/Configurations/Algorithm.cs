using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Configurations
{
    public class AlgorithmConfiguration : IEntityTypeConfiguration<Algorithm>
    {
        public void Configure(EntityTypeBuilder<Algorithm> builder)
        {
            builder.ToTable("algorithms");

            builder.HasKey(a => a.Id).HasName("PRIMARY");

            builder.Property(a => a.Name)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(a => a.ClassName)
                .HasColumnName("class_name")
                .HasColumnType("varchar(64)")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(a => a.FileName)
                .HasColumnName("file_name")
                .HasColumnType("varchar(32)")
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(a => a.IsRemoveable)
                .HasColumnName("is_removeable")
                .HasColumnType("tinyint(1) unsigned");
        }
    }
}
