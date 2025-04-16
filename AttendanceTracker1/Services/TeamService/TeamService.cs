using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Services.TeamService
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TeamService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<ApiResponse<object>> GetAllTeams(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var totalRecords = await _context.Teams.CountAsync();

            var teams = await _context.Teams
                .OrderBy(t => t.Id) // Stable ordering
                .Skip(skip) // Skip the records for the previous pages
                .Take(pageSize) // Limit the number of records to the page size
                .Select(t => new TeamResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return ApiResponse<object>.Success(new
            {
                teams,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Request successful.");
        }

        public async Task<ApiResponse<object>> GetTeamById(int id)
        {

            var teamResponse = await _context.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamResponse
                {
                    Id = t.Id,
                    Name = t.Name
                }).FirstOrDefaultAsync();

            return ApiResponse<object>.Success(teamResponse);
        }

        public async Task<ApiResponse<object>> AddTeam(AddTeamDto addTeamDto)
        {
            // Optional: Check for duplicate team names
            var exists = await _context.Teams.AnyAsync(t => t.Name == addTeamDto.Name);
            if (exists)
            {
                return ApiResponse<object>.Failed("A team with this name already exists.");
            }

            var newTeam = new Team
            {
                Name = addTeamDto.Name
            };

            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Success(new TeamResponse
            {
                Id = newTeam.Id,
                Name = newTeam.Name
            }, "Team successfully created.");
        }

        public async Task<ApiResponse<object>> UpdateTeam(int id, AddTeamDto addTeamDto)
        {
            // Check if the team exists
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
                return ApiResponse<object>.Failed("Team not found.");

            // Optional: Prevent renaming to an existing team name
            var nameExists = await _context.Teams
                .AnyAsync(t => t.Name == addTeamDto.Name && t.Id != id);

            if (nameExists)
                return ApiResponse<object>.Failed("Another team with this name already exists.");

            // Update team properties
            team.Name = addTeamDto.Name;

            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Success(new TeamResponse
            {
                Id = team.Id,
                Name = team.Name
            }, "Team updated successfully.");
        }
        public async Task<ApiResponse<object>> AssignTeam(int userId, int teamId)
        {
            // Check if the user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return ApiResponse<object>.Failed("User not found.");

            // Check if the team exists
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return ApiResponse<object>.Failed("Team not found.");

            // Check if the user is already assigned to the team
            var isAssigned = await _context.UserTeams
                .AnyAsync(ut => ut.UserId == userId && ut.TeamId == teamId);

            if (isAssigned)
                return ApiResponse<object>.Failed("User is already assigned to this team.");

            // Create a new UserTeam entry
            var userTeam = new UserTeam
            {
                UserId = userId,
                TeamId = teamId,
                AssignedAt = DateTime.UtcNow // Store when the user was assigned to the team
            };

            _context.UserTeams.Add(userTeam);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Success(null, "User successfully assigned to the team.");
        }
        public async Task<ApiResponse<object>> UpdateTeamAssignment(int id)
        {
            return ApiResponse<object>.Success(null, "This function is not implemented yet.");
        }
        public async Task<ApiResponse<object>> DisableTeam(int id)
        {
            return ApiResponse<object>.Success(null, "This function is not implemented yet.");
        }
        public async Task<ApiResponse<object>> EnableTeam(int id)
        {
            return ApiResponse<object>.Success(null, "This function is not implemented yet.");
        }
    }
}
