using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Configurations
{
    public class SessionTestResultConfiguration : IEntityTypeConfiguration<SessionTestResult>
    {
        public void Configure(EntityTypeBuilder<SessionTestResult> builder)
        {
            builder.ToTable("session_test_results");

            builder.HasIndex(e => e.SessionTestId, "session_test_results_ibfk_1");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.SessionTestId)
                .HasColumnName("session_test_id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.XBest)
                .HasColumnName("x_best")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(e => e.FBest)
                .HasColumnName("f_best")
                .HasColumnType("float");

            builder.Property(e => e.FitnessFunctionEvaluations)
                .HasColumnName("fitness_function_evaluations")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.Dimension)
                .HasColumnName("dimension")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.ParametersGrid)
                .HasColumnName("parameters_grid")
                .HasColumnType("text")
                .HasDefaultValue("{}");

            builder.HasOne(d => d.SessionTest)
                .WithMany(p => p.Results)
                .HasForeignKey(d => d.SessionTestId)
                .HasConstraintName("session_test_results_ibfk_1");
        }
    }
}
