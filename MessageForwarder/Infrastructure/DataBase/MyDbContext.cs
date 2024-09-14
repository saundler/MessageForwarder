using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;


namespace MessageForwarder.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<BadWord> BadWords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=identifier.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BadWord>()
                .HasIndex(bw => bw.Word)
                .IsUnique(); // Устанавливаем уникальный индекс для столбца Word
        }
    }

    [Table("BadWords")]
    public class BadWord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Word { get; set; }
    }
}