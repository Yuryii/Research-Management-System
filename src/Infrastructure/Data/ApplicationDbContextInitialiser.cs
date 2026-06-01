using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RMS.Application.Common.Options;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Domain.ValueObjects;
using RMS.Infrastructure.Identity;
using DomainApplication = RMS.Domain.Entities.Models.Application;
namespace RMS.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly DefaultStepIdsOptions _stepIds;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<DefaultStepIdsOptions> stepIds)
    {
        _logger = logger;
        _context = context;
        _environment = environment;
        _userManager = userManager;
        _roleManager = roleManager;
        _stepIds = stepIds.Value;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_environment.IsDevelopment())
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
            }
            else if (_environment.IsStaging())
            {
                await _context.Database.MigrateAsync();
            }
            else
            {
                await _context.Database.EnsureCreatedAsync();
            }

            _logger.LogInformation("Database schema ensured for {EnvironmentName}.", _environment.EnvironmentName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Tasks",
                Colour = Colour.Green,
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });
        }
        // Defauld Steps
        var teacherInitialStepId = _stepIds.TeacherStepId;
        var dvqlttInitialStepId = _stepIds.DvqlttStepId;
        var tttvStepId = _stepIds.TttvStepId;
        var dvqlttReviewStepId = _stepIds.DvqlttReviewStepId;
        var khcnHtqtStepId = _stepIds.KhcnHtqtStepId;
        var returnedStepId = _stepIds.ReturnedStepId;

        if (!_context.Steps.Any())
        {
            var steps = new List<Step>
            {
                new Step
                {
                    Id = teacherInitialStepId,
                    Name = "Đang xử lý bởi Giảng viên",
                    ShortName = "Đang xử lý bởi GV",
                    Order = 0,
                    StepDetails = new List<StepDetail>
                    {
                        new StepDetail
                        {
                            Id = Guid.Parse("343B1904-AB23-42D4-80DB-760E93F15B09"),
                            Name = "Giảng viên đang chuẩn bị hồ sơ",
                            Order = 0,
                            NextStepDetailId = Guid.Parse("11111111-1111-1111-1111-111111111101")
                        }
                    }
                },
                new Step
                {
                    Id = dvqlttInitialStepId,
                    Name = "Đang xử lý bởi đơn vị quản lý trực tiếp",
                    ShortName = "Đang xử lý bởi DVQLTT",
                    Order = 1,
                    StepDetails = new List<StepDetail>()
                    {
                        new StepDetail()
                        {
                            Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                            Name = "ĐVQLTT đang kiểm tra sơ lược",
                            Order = 1
                        },
                        new StepDetail()
                        {
                            Name = "ĐVQLTT đang xác nhận phiếu đề nghị",
                            Order = 2
                        },
                        new StepDetail()
                        {
                            Name = "ĐVQLTT chuyển bài báo công bố cho TTTV",
                            Order = 3,
                            NextStepDetailId = Guid.Parse("9454C8AC-B21D-4124-A0A0-201CF145E92A")
                        }
                    }
                },
                new Step
                {
                    Id = tttvStepId,
                    Name = "Đang xử lý bởi Trung tâm thư viện",
                    ShortName = "Đang xử lý bởi TTTV",
                    Order = 2,
                    StepDetails = new List<StepDetail>()
                    {
                        new StepDetail()
                        {
                            Id = Guid.Parse("9454C8AC-B21D-4124-A0A0-201CF145E92A"),
                            Name = "TTTV đang kiểm tra trùng lặp",
                            Order = 1
                        },
                        new StepDetail()
                        {
                            Name = "TTTV đang xác nhận nộp lưu chiểu",
                            Order = 2,
                            NextStepDetailId = Guid.Parse("993B42E9-2A46-445D-BDBA-9C4551353BE6")
                        }
                    }
                },
                new Step
                {
                    Id = dvqlttReviewStepId,
                    Name = "Đang xử lý bởi đơn vị quản lý trực tiếp",
                    ShortName = "Đang xử lý bởi DVQLTT",
                    Order = 3,
                    StepDetails = new List<StepDetail>()
                    {
                        new StepDetail()
                        {
                            Id = Guid.Parse("993B42E9-2A46-445D-BDBA-9C4551353BE6"),
                            Name = "ĐVQLTT đã nhận biên bản trùng lặp và giấy xác nhận lưu chiểu",
                            Order = 1
                        },
                        new StepDetail()
                        {
                            Name = "TTTV đang xác nhận nộp lưu chiểu",
                            Order = 2,
                            NextStepDetailId = Guid.Parse("C9B1F8A7-5B3B-4E5B-9C0D-1F2E3A4B5C6D")
                        }
                    }
                },
                new Step
                {
                    Id = khcnHtqtStepId,
                    Name = "Đang xử lý bởi phòng khoa học công nghệ - hợp tác quốc tế",
                    ShortName = "Đang xử lý bởi KHCN-HTQT",
                    Order = 4,
                     StepDetails = new List<StepDetail>()
                    {
                        new StepDetail()
                        {
                            Id = Guid.Parse("CF22AED9-0B93-4D2F-9D03-0A4C2485DABE"),
                            Name = "KNCH-HTQT đã tiếp nhân hồ sơ",
                            Order = 1
                        },
                        new StepDetail()
                        {
                            Name = "KNCH-HTQT đang kiểm tra chi tiết",
                            Order = 2
                        },
                        new StepDetail()
                        {
                            Name = "KNCH-HTQT đã ra biên bản nghiệm thu",
                            Order = 3
                        },
                        new StepDetail()
                        {
                            Name = "KNCH-HTQT đã tính số tiết NCKH",
                            Order = 4
                        }
                    }
                },
                new Step
                {
                    Id = returnedStepId,
                    Name = "Hồ sơ đã bị trả về",
                    Order = 5,
                    StepDetails = new List<StepDetail>()
                    {
                        new StepDetail()
                        {
                            Id = Guid.Parse("C9B1F8A7-5B3B-4E5B-9C0D-1F2E3A4B5C6D"),
                            Name = "Hồ sơ đã bị trả về",
                            Order = 1,
                            IsReturnStep = true
                        }
                    }
                }
            };
            _context.Steps.AddRange(steps);
            await _context.SaveChangesAsync();
        }

        // Default roles and users are seeded after steps so RoleStepPermissions can be seeded immediately afterwards.
        var roles = new[]
        {
            Roles.Administrator,
            Roles.Teacher,
            Roles.Tttv,
            Roles.Dvqltt,
            Roles.KhcnHtqt
        };

        foreach (var roleName in roles)
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }

            var userName = $"{roleName.ToLowerInvariant()}@123";
            var (firstName, lastName) = roleName switch
            {
                Roles.Administrator => ("Nguyen", "Van Admin"),
                Roles.Teacher => ("Nguyen", "Giang Vien"),
                Roles.Tttv => ("Tran", "Thu Truong"),
                Roles.Dvqltt => ("Le", "Dao Vien"),
                Roles.KhcnHtqt => ("Pham", "Quan Ly"),
                _ => ("Ho", "Ten Mac Dinh")
            };
            var user = new ApplicationUser { UserName = userName, Email = userName, FirstName = firstName, LastName = lastName };

            if (_userManager.Users.All(u => u.UserName != user.UserName))
            {
                await _userManager.CreateAsync(user, $"{roleName}@123");
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }

        var roleStepMappings = new Dictionary<string, Guid[]>
        {
            [Roles.Administrator] = [teacherInitialStepId, dvqlttInitialStepId, tttvStepId, dvqlttReviewStepId, khcnHtqtStepId, returnedStepId],
            [Roles.Teacher] = [teacherInitialStepId],
            [Roles.Dvqltt] = [dvqlttInitialStepId, dvqlttReviewStepId],
            [Roles.Tttv] = [tttvStepId],
            [Roles.KhcnHtqt] = [khcnHtqtStepId]
        };

        _logger.LogInformation("RoleStepPermissions seed: Found {StepCount} step IDs across all roles", roleStepMappings.Values.SelectMany(v => v).Distinct().Count());

        var rolesByName = await _roleManager.Roles
            .Where(role => role.Name != null && roleStepMappings.Keys.Contains(role.Name))
            .ToDictionaryAsync(role => role.Name!);

        _logger.LogInformation("RoleStepPermissions seed: Found {RoleCount} matching roles: {Roles}", rolesByName.Count, string.Join(", ", rolesByName.Keys));

        var allStepIds = roleStepMappings.Values.SelectMany(ids => ids).Distinct().ToList();
        var stepsById = await _context.Steps
            .Where(s => allStepIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        _logger.LogInformation("RoleStepPermissions seed: Found {StepDbCount} steps in DB matching expected IDs (expected {StepExpectedCount})", stepsById.Count, allStepIds.Count);

        var roleStepPermissions = roleStepMappings
            .Where(mapping => rolesByName.ContainsKey(mapping.Key))
            .SelectMany(mapping => mapping.Value
                .Where(stepId => stepsById.ContainsKey(stepId))
                .Select(stepId => new RoleStepPermission
                {
                    RoleId = rolesByName[mapping.Key].Id,
                    StepId = stepId,
                    Step = stepsById[stepId]
                }))
            .ToList();

        _logger.LogInformation("RoleStepPermissions seed: Generated {Count} permission records to insert", roleStepPermissions.Count);

        foreach (var roleStepPermission in roleStepPermissions)
        {
            if (!_context.RoleStepPermissions.Any(existingPermission =>
                    existingPermission.RoleId == roleStepPermission.RoleId &&
                    existingPermission.StepId == roleStepPermission.StepId))
            {
                _context.RoleStepPermissions.Add(roleStepPermission);
            }
        }

        _logger.LogInformation("RoleStepPermissions seed: ChangeTracker has {Count} pending additions before SaveChanges", _context.ChangeTracker.Entries<RoleStepPermission>().Count(e => e.State == EntityState.Added));
        await _context.SaveChangesAsync();
        _logger.LogInformation("RoleStepPermissions seed: SaveChanges completed. Total RoleStepPermissions in DB now: {Total}", await _context.RoleStepPermissions.CountAsync());

        if (!_context.Applications.Any())
        {
            var teacherStepDetailId = _context.StepDetails
                .Where(sd => sd.Step.Order == 0)
                .OrderBy(sd => sd.Step.Order)
                .ThenBy(sd => sd.Order)
                .Select(sd => sd.Id)
                .First();
            var dvqlttStepDetailId = _context.StepDetails
                .Where(sd => sd.Step.Order == 1)
                .OrderBy(sd => sd.Step.Order)
                .ThenBy(sd => sd.Order)
                .Select(sd => sd.Id)
                .First();
            var tttvStepDetailId = _context.StepDetails
                .Where(sd => sd.Step.Order == 2)
                .OrderBy(sd => sd.Step.Order)
                .ThenBy(sd => sd.Order)
                .Select(sd => sd.Id)
                .First();

            var teacherApplicationId = Guid.Parse("34BFFA1E-8D68-440D-80BB-2863614B7C50");
            var dvqlttApplicationId = Guid.Parse("3C8C8FE4-36F8-4D45-B46B-AC705D028770");
            var tttvApplicationId = Guid.Parse("ABC1743B-3DB7-4721-A859-1462572AC193");

            // Look up user IDs for CreatedBy
            var teacherUser = await _userManager.FindByNameAsync("teacher@123");
            var tttvUser = await _userManager.FindByNameAsync("tttv@123");
            var dvqlttUser = await _userManager.FindByNameAsync("dvqltt@123");

            var applications = new List<DomainApplication>
            {
                new()
                {
                    Id = tttvApplicationId,
                    Code = "APP-SEED-001",
                    Title = "Hồ sơ đề nghị công nhận bài báo khoa học",
                    Description = "Hồ sơ mẫu được tạo sẵn để kiểm thử luồng xử lý hồ sơ nghiên cứu khoa học.",
                    Status = ApplicationStatus.Submitted,
                    StepDetailId = tttvStepDetailId,
                    CreatedBy = teacherUser?.Id,
                },
                new()
                {
                    Id = dvqlttApplicationId,
                    Code = "APP-SEED-002",
                    Title = "Hồ sơ kiểm tra xác nhận đơn vị quản lý trực tiếp",
                    Description = "Dữ liệu mẫu phục vụ kiểm tra luồng xử lý hồ sơ tại đơn vị quản lý trực tiếp.",
                    Status = ApplicationStatus.Submitted,
                    StepDetailId = dvqlttStepDetailId,
                    CreatedBy = dvqlttUser?.Id,
                },
                new()
                {
                    Code = "APP-SEED-003",
                    Title = "Hồ sơ xác nhận giờ nghiên cứu khoa học",
                    Description = "Dữ liệu mẫu phục vụ kiểm tra chức năng danh sách, chi tiết và cập nhật trạng thái hồ sơ.",
                    Status = ApplicationStatus.Submitted,
                    StepDetailId = tttvStepDetailId,
                    CreatedBy = teacherUser?.Id,
                },
                new()
                {
                    Id = teacherApplicationId,
                    Code = "APP-SEED-004",
                    Title = "Hồ sơ xác nhận Giảng viên",
                    Description = "Dữ liệu mẫu phục vụ kiểm tra Giảng viên",
                    Status = ApplicationStatus.Draft,
                    StepDetailId = teacherStepDetailId,
                    CreatedBy = teacherUser?.Id,
                }
            };

            _context.Applications.AddRange(applications);
            await _context.SaveChangesAsync();

            var files = new List<RMS.Domain.Entities.Models.File>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "teacher-attachment-1.pdf",
                    Path = "/uploads/seed/teacher/teacher-attachment-1.pdf",
                    ContentType = "application/pdf",
                    Length = 245760
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "tttv-attachment-1.pdf",
                    Path = "/uploads/seed/tttv/tttv-attachment-1.pdf",
                    ContentType = "application/pdf",
                    Length = 512000
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "dvqltt-attachment-1.pdf",
                    Path = "/uploads/seed/dvqltt/dvqltt-attachment-1.pdf",
                    ContentType = "application/pdf",
                    Length = 384000
                }
            };

            _context.Files.AddRange(files);
            await _context.SaveChangesAsync();

            var dvqlttStepIdForFile = _context.Steps.Where(s => s.Order == 1).Select(s => s.Id).First();
            var tttvStepIdForFile = _context.Steps.Where(s => s.Order == 2).Select(s => s.Id).First();

            var applicationFiles = new List<ApplicationFile>
            {
                new()
                {
                    ApplicationId = tttvApplicationId,
                    FileId = files[0].Id,
                    StepId = dvqlttStepIdForFile
                },
                new()
                {
                    ApplicationId = tttvApplicationId,
                    FileId = files[1].Id,
                    StepId = tttvStepIdForFile
                },
                new()
                {
                    ApplicationId = dvqlttApplicationId,
                    FileId = files[2].Id,
                    StepId = dvqlttStepIdForFile
                }
            };

            _context.ApplicationFiles.AddRange(applicationFiles);
            await _context.SaveChangesAsync();
        }

    }
}
