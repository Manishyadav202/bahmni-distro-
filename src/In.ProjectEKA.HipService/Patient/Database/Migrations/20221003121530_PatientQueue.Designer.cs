// <auto-generated />

using In.ProjectEKA.HipService.Patient.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace In.ProjectEKA.HipService.Migrations
{
    [DbContext(typeof(PatientContext))]
    [Migration("20221003121530_PatientQueue")]
    partial class PatientQueue
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("In.ProjectEKA.HipService.Patient.PatientQueue", b =>
            {
                b.Property<string>("RequestId")
                    .HasColumnType("text");
                
                b.Property<string>("DateTimeStamp")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("text")
                    .HasDefaultValueSql("now()");
                
                b.Property<string>("HipCode")
                    .HasColumnType("text");
                
                b.Property<string>("Profile")
                    .HasColumnType("text");
                
                b.HasKey("RequestId");

                b.ToTable("PatientQueue");
            });
#pragma warning restore 612, 618
        }
    }
}