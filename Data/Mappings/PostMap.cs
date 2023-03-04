using BlogAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Data.Mappings
{
    public class PostMap : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Post");

            //Primary Key
            builder.HasKey(x=>x.Id);

            //Identity
            builder.Property(x=>x.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            //Propriedades específicas
            builder.Property(x=>x.LastUpdateDate)
                .IsRequired()
                .HasColumnName("LastUpdateDate")
                .HasColumnType("SMALLDATETIME")
                .HasMaxLength(60)
                .HasDefaultValueSql("GETDATE()");
                // .HasDefaultValue(DateTime.Now.ToUniversalTime());

            //Índices
            builder.HasIndex(x=>x.Slug, "IX_Post_Slug")
                .IsUnique();

            //Relacionamentos 1:N
            builder
                .HasOne(x=>x.Author)
                .WithMany(x=>x.Posts)
                .HasConstraintName("FK_Post_Author")
                .OnDelete(DeleteBehavior.Cascade);
            
            builder
                .HasOne(x=>x.Category)
                .WithMany(x=>x.Posts)
                .HasConstraintName("FK_Post_Category")
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.Tags)
                .WithMany(x => x.Posts)
                .UsingEntity<Dictionary<string, object>>(
                    "PostTag",
 post => post // Inicia com Post
     .HasOne<Tag>() // Post possui uma tag
     .WithMany()
     .HasForeignKey("PostId") // Define a chave estrangeira
     .HasConstraintName("FK_PostRole_PostId") // Aqui está errado o nom da constraint, mas é só o nome
     .OnDelete(DeleteBehavior.Cascade),
 tag => tag // Mapeaia a tag
     .HasOne<Post>() // Que tem um post
     .WithMany() // Que contém muitas tags
     .HasForeignKey("TagId") // Chave estrangeira
     .HasConstraintName("FK_PostTag_TagId")
     .OnDelete(DeleteBehavior.Cascade));

        }
    }
}