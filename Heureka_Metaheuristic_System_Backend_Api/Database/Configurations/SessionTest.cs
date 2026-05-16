using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Configurations
{
    public class SessionTestConfiguration : IEntityTypeConfiguration<SessionTest>
    {
        public void Configure(EntityTypeBuilder<SessionTest> builder)
        {
            builder.ToTable("session_tests");

            builder.HasIndex(e => e.AlgorithmId, "session_tests_ibfk_1");
            builder.HasIndex(e => e.FitnessFunctionId, "session_tests_ibfk_2");
            builder.HasIndex(e => e.SessionId, "session_tests_ibfk_3");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.SessionId)
                .HasColumnName("session_id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.AlgorithmId)
                .HasColumnName("algorithm_id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.FitnessFunctionId)
                .HasColumnName("fitness_function_id")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.TestInvokePerParameters)
                .HasColumnName("test_invoke_per_parameters")
                .HasColumnType("int(10) unsigned");

            builder.Property(e => e.ParametersConfig)
                .HasColumnName("parameters_config")
                .HasColumnType("text")
                .HasDefaultValue("{}");

            builder.Property(e => e.Progress)
                .HasColumnName("progress")
                .HasColumnType("double unsigned");

            builder.HasOne(d => d.Algorithm)
                .WithMany(p => p.SessionTests)
                .HasForeignKey(d => d.AlgorithmId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_tets_ibfk_1");

            builder.HasOne(d => d.FitnessFunction)
                .WithMany(p => p.SessionTests)
                .HasForeignKey(d => d.FitnessFunctionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_tets_ibfk_2");

            builder.HasOne(d => d.Session)
                .WithMany(p => p.Tests)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_tets_ibfk_3");
        }
    }
}
