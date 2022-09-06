using Microsoft.EntityFrameworkCore;
using StorytellerBot.Models;

namespace StorytellerBot.Data;

public class AdventureRepository
{
    private readonly AdventureContext _context;
    public AdventureRepository(AdventureContext context)
    {
        _context = context;
    }

    #region User
    public async Task<User?> GetUserAsync(long userId) =>
        await _context.Users.Include(u => u.SavedGames).FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<User> GetOrCreateUserAsync(long userId)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
        {
            user = new User { Id = userId };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    #endregion

    #region SavedStatus

    public async Task UpdateSavedStatusAsync(SavedStatus? savedStatus, string storyState, DateTime utcNow)
    {
        if (savedStatus == null)
        {
            return;
        }
        savedStatus.StoryState = storyState;
        _context.SavedStatuses.Update(savedStatus);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSavedStatusAsync(SavedStatus savedStatus)
    {
        _context.SavedStatuses.Remove(savedStatus);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region CommandProgress

    public async Task<CommandProgress?> GetCommandProgressForUserAsync(long userId) =>
        await _context.CommandProgresses.FirstOrDefaultAsync(cp => cp.UserId == userId);

    public async Task CreateCommandProgressAsync(CommandProgress newCommandProgress)
    {
        _context.CommandProgresses.Add(newCommandProgress);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCommandProgressAsync(
        CommandProgress commandProgress, string step, object? argument = null)
    {
        commandProgress.Step = step;
        commandProgress.Argument = argument?.ToString();
        _context.CommandProgresses.Update(commandProgress);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommandProgressAsync(CommandProgress? commandProgress)
    {
        if (commandProgress == null)
        {
            return;
        }
        _context.CommandProgresses.Remove(commandProgress);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region CurrentGame

    public async Task<CurrentGame?> GetCurrentGameForUserAsync(long userId)
    {
        return await _context.CurrentGames
            .Include(cg => cg.SavedStatus.Adventure)
            .FirstOrDefaultAsync(cg => cg.UserId == userId);
    }

    public async Task DeleteCurrentGameAsync(CurrentGame? currentGame)
    {
        if (currentGame == null)
        {
            return;
        }
        _context.CurrentGames.Remove(currentGame);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Mixed

    public async Task StartGameAsync(User user, int adventureId, DateTime utcNow)
    {
        var newSavedStatus = new SavedStatus
        {
            AdventureId = adventureId,
            UserId = user.Id,
            LastUpdated = utcNow,
        };
        _context.SavedStatuses.Add(newSavedStatus);
        await _context.SaveChangesAsync();

        _context.CurrentGames.Add(new CurrentGame { UserId = user.Id, SavedStatusId = newSavedStatus.Id });
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Adventure

    internal async Task<List<Adventure>> GetAllAdventuresAsync() =>
        await _context.Adventures.AsNoTracking().ToListAsync();

    internal async Task<bool> AdventureExistsAsync(int adventureId) =>
        await _context.Adventures.AsNoTracking().AnyAsync(a => a.Id == adventureId);

    #endregion
}
