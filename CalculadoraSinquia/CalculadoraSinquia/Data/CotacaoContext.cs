using CalculadoraSinqia.Models;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraSinqia.Data
{
    public class CotacaoContext : DbContext
    {
        public DbSet<CotacaoItem> Cotacao { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Substitua pela sua string de conexão real do MySQL
            string connectionString = "Server=localhost;Port=3306;Database='testesqia';Uid=root;Pwd=testesqia;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

}