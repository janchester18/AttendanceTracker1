using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services.TeamService
{
    public interface ITeamService
    {
        public Task<ApiResponse<object>> GetAllTeams(int page, int pageSize);
        public Task<ApiResponse<object>> GetTeamById(int id);
        public Task<ApiResponse<object>> AddTeam(AddTeamDto addTeamDto); // notify concerned user and all admins when late
        public Task<ApiResponse<object>> UpdateTeam(int id, AddTeamDto addTeamDto); // notify  concerned user and all admins when early out
        public Task<ApiResponse<object>> AssignTeam(int userId, int teamId);
        public Task<ApiResponse<object>> UpdateTeamAssignment(int id); // notify concerned user and all admins when break exceeded the configured time
        public Task<ApiResponse<object>> DisableTeam(int id); // notify the concerned user and all admins
        public Task<ApiResponse<object>> EnableTeam(int id);
    }
}
