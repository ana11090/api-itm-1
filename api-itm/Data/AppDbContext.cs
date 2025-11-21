using api_itm.Data.Entity.api_itm.Domain.Employees;
using api_itm.Data.Entity.Ru;
using api_itm.Data.Entity.Ru.Contracts;
using api_itm.Data.Entity.Ru.Contracts.api_itm.Data.Entity.Ru.Lookups;
using api_itm.Data.Entity.Ru.Contracts.ContractsSuspended;
using api_itm.Data.Entity.Ru.Contracts.ContractsSuspended.Medical;
using api_itm.Data.Entity.Ru.Contracts.Work;
using api_itm.Data.Entity.Ru.Disability;
using api_itm.Data.Entity.Ru.ForginerWorkAuthorization;
using api_itm.Data.Entity.Ru.Geography;
using api_itm.Data.Entity.Ru.Reges;
using api_itm.Data.Entity.Ru.Salary;
using api_itm.Data.Entity.Ru.Study;
using api_itm.Models.Contracts.Envelope;
using api_itm.Models.View;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace api_itm
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } 
        public DbSet<DisabilityType> DisabilityTypes { get; set; }
        public DbSet<DisabilityGrade> DisabilityGrades { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<TypePapartid> TypePapartide { get; set; }

        public DbSet<NationalityType> NationalityTypes { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<IdentityDocumentType> IdentityDocumentTypes { get; set; }

        public DbSet<PostPerson> PostPeople => Set<PostPerson>(); // DbSet for PostPerson entity from sheme "ru"
        public DbSet<DisabilityCharacter> DisabilityCharacters => Set<DisabilityCharacter>(); // DbSet for County entity from sheme "ru"

        public DbSet<RegesSync> RegesSyncs => Set<RegesSync>(); // DbSet for RegesSync entity from sheme "ru"
        public DbSet<WorkPermitType> WorkPermitTypes => Set<WorkPermitType>(); // DbSet for Employee entity from sheme "api_itm"
        public DbSet<ContractsRu> ContractsRu => Set<ContractsRu>();
        public DbSet<EndDateException> EndDateExceptions => Set<EndDateException>();

        public DbSet<SporTip> TypeSpor => Set<SporTip>();
        public DbSet<ContractState> ContractsState => Set<ContractState>();
        public DbSet<FunctionStat> FunctionsStat => Set<FunctionStat>(); //FunctionStat

        public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>(); //FunctionStat

        public DbSet<ContractBonusesView> ContractsBonusesView => Set<ContractBonusesView>(); //ContractBonusesView
        public DbSet<ContractBonuses> ContractsBonuses => Set<ContractBonuses>(); //
        public DbSet<ContractSalaryBonus> ContractsSalaryBonuses => Set<ContractSalaryBonus>(); //ContractSalaryBonuses
        public DbSet<WorkSchedule> WorkScheduleNorm => Set<WorkSchedule>(); //WorkSchedule
        public DbSet<WorkingTimeInterval> WorkingTimeIntervals => Set<WorkingTimeInterval>(); //WorkingTimeInterval
        public DbSet<WorkTimeAllocation> WorkTimeAllocation => Set<WorkTimeAllocation>();//WorkTimeAllocation
        public DbSet<WorkDistributionId> WorkDistributionId => Set<WorkDistributionId>();//WorkDistributionId
        public DbSet<ShiftType> ShiftType => Set<ShiftType>();//ShiftType
        public DbSet<TypeContractRu> TypeContractRu => Set<TypeContractRu>();//TypeContractRu
        public DbSet<ContractTypeDuration> ContractTypeDuration => Set<ContractTypeDuration>();//ContractTypeDuration
        public DbSet<WorkNormType> WorkNormType => Set<WorkNormType>();//WorkNormType
        public DbSet<County> County => Set<County>();//County
        public DbSet<RegesContractSync> RegesContractSync => Set<RegesContractSync>(); //RegesContractSync

        public DbSet<WorkLocationType> WorkLocationType => Set<WorkLocationType>(); //WorkLocationType

        public DbSet<RegesCor> RegesCor => Set<RegesCor>();//RegesCor   
        public DbSet<DisabilityInvalidityGrade> DisabilityInvalidityGrade => Set<DisabilityInvalidityGrade>(); //RegesCor   

        public DbSet<RegesSyncOpenrationsEmployee> RegesSyncOpenrationsEmployee => Set<RegesSyncOpenrationsEmployee>(); //RegesSyncModificationEmployee
        public DbSet<RegesSyncModificationContracts> RegesSyncModificationContracts => Set<RegesSyncModificationContracts>(); //RegesSyncModificationContracts
        public DbSet<City> City => Set<City>(); //City
        public DbSet<CountyCity> CountyCity => Set<CountyCity>(); //CountyCity

        public DbSet<ContractRuSuspended> ContractRuSuspended => Set<ContractRuSuspended>(); //ContractRuSuspended
        public DbSet<SuspensionLegalGround> SuspensionLegalGround => Set<SuspensionLegalGround>(); //SuspensionLegalGround
        public DbSet<MedicalLeaveEntry> MedicalLeaveEntry => Set<MedicalLeaveEntry>(); //SuspensionLegalGround

        public DbSet<FunctieReges> FunctieReges => Set<FunctieReges>(); //FunctieReges, "functiereges"
        public DbSet<FunctionLeadership> FunctionLeadership => Set<FunctionLeadership>();  //FunctionLeadership, "functie_conducere"
        public DbSet<StateFunctions> StateFunctions => Set<StateFunctions>();  //StateFunctions, "statfunctii"
        public DbSet<ProfessionalGrade> ProfessionalGrade => Set<ProfessionalGrade>(); //ProfessionalGrade, "gradprofesional"
        public DbSet<ServiceFunction> ServiceFunction => Set<ServiceFunction>();
        public DbSet<CategoryPost> CategoryPost => Set<CategoryPost>(); //CategoryPost
        public DbSet<Post> Post => Set<Post>();//Post

        public DbSet<L153Keys> L153Keys => Set<L153Keys>(); // in views
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
         .ToTable("utilizator", schema: "ru") // PostgreSQL table name
         .HasKey(u => u.IdUser); // C# key


            modelBuilder.Entity<User>().Property(u => u.IdUser).HasColumnName("idutilizator");
            modelBuilder.Entity<User>().Property(u => u.Username).HasColumnName("userutilizator");
            modelBuilder.Entity<User>().Property(u => u.Password).HasColumnName("parolautilizator"); 
            modelBuilder.Entity<User>()
         .ToTable("utilizator") // PostgreSQL table name
         .HasKey(u => u.IdUser); // C# key

            modelBuilder.Entity<L153Keys>().HasNoKey();

            base.OnModelCreating(modelBuilder);

            // Auto-apply all IEntityTypeConfiguration<T> in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
