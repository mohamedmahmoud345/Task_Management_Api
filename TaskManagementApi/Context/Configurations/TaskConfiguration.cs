using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Api.Model;
using TaskManagement.Api.Enums;

namespace TaskManagement.Api.Context.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<TaskData>
    {
        public void Configure(EntityTypeBuilder<TaskData> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Priority)
                .HasConversion<int>();

            builder.Property(x => x.Status)
                .HasConversion<int>();

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.Priority);
            builder.HasIndex(x => x.DueDate);
            builder.HasIndex(x => new { x.DueDate, x.Status });

            builder.HasOne(x => x.User)
                .WithMany(y => y.Tasks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
