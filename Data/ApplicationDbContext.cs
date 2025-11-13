using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkAcademy.Models;

namespace WorkAcademy.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Vaga> Vagas { get; set; }
        public DbSet<Certificado> Certificados { get; set; }
        public DbSet<Publicacao> Publicacoes { get; set; }
        public DbSet<Conexao> Conexoes { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<CurtidaComentario> CurtidasComentario { get; set; }
        public DbSet<PublicacaoCurtida> PublicacaoCurtidas { get; set; }
        public DbSet<CursoUsuario> CursosUsuarios { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CurtidaComentario>()
                .HasIndex(c => new { c.UsuarioId, c.ComentarioId })
                .IsUnique();

            modelBuilder.Entity<CurtidaComentario>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurtidaComentario>()
                .HasOne(c => c.Comentario)
                .WithMany(c => c.Curtidas)
                .HasForeignKey(c => c.ComentarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.CPF).IsUnique();

            modelBuilder.Entity<Empresa>()
                .HasIndex(e => e.Email).IsUnique();
            modelBuilder.Entity<Empresa>()
                .HasIndex(e => e.CNPJ).IsUnique();

            modelBuilder.Entity<Vaga>()
                .HasOne(v => v.Empresa)
                .WithMany(e => e.Vagas)
                .HasForeignKey(v => v.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Conexao>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conexao>()
                .HasOne(c => c.ConectadoCom)
                .WithMany()
                .HasForeignKey(c => c.ConectadoComId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Publicacao)
                .WithMany(p => p.Comentarios)
                .HasForeignKey(c => c.PublicacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Curso>()
                .Property(c => c.Valor)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vaga>()
                .Property(v => v.Salario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PublicacaoCurtida>()
                .HasOne(pc => pc.Publicacao)
                .WithMany(p => p.Curtidas)
                .HasForeignKey(pc => pc.PublicacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PublicacaoCurtida>()
                .HasOne(pc => pc.Usuario)
                .WithMany()
                .HasForeignKey(pc => pc.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CursoUsuario>()
                .HasIndex(cu => new { cu.UsuarioId, cu.CursoId }).IsUnique();

            modelBuilder.Entity<Notification>(e =>
            {
                e.ToTable("Notifications");        // nome da tabela no SQL
                e.HasKey(n => n.Id);

                e.Property(n => n.UserId)
                 .IsRequired();

                e.Property(n => n.Message)
                 .IsRequired()
                 .HasMaxLength(500);

                e.Property(n => n.IsRead)
                 .HasDefaultValue(false);

                e.Property(n => n.IsArchived)
                 .HasDefaultValue(false);

                e.Property(n => n.CreatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                // Se quiser integridade com AspNetUsers:
                e.HasOne<IdentityUser>()
                 .WithMany()
                 .HasForeignKey(n => n.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // índice útil para caixa de notificações
                e.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
            });

        }
    }
}
