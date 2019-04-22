﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using marcu5yolo.Models;

namespace marcu5yolo.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.8-servicing-32085")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("marcu5yolo.Models.MessageModel", b =>
                {
                    b.Property<int>("MessageModelId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("filename1");

                    b.Property<string>("filename2");

                    b.Property<string>("guid");

                    b.Property<string>("intendedClient");

                    b.Property<string>("uri1");

                    b.Property<string>("uri2");

                    b.HasKey("MessageModelId");

                    b.ToTable("entry");
                });
#pragma warning restore 612, 618
        }
    }
}
