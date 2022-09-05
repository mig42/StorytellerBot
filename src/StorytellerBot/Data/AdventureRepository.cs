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
    public async Task<User?> GetUserAsync(long userId) => await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

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

    public async Task UpdateSavedStatusAsync(SavedStatus savedStatus, string storyState, DateTime utcNow)
    {
        savedStatus.StoryState = storyState;
        _context.SavedStatuses.Update(savedStatus);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region CommandProgress
    public async Task ReplaceCommandProgressAsync(User user, CommandProgress newCommandProgress)
    {
        if (user.CommandProgress != null)
        {
            _context.CommandProgresses.Remove(user.CommandProgress);
        }

        user.CommandProgress = newCommandProgress;
        _context.CommandProgresses.Add(newCommandProgress);
        await _context.SaveChangesAsync();
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

    public async Task DeleteCommandProgressAsync(CommandProgress commandProgress)
    {
        _context.CommandProgresses.Remove(commandProgress);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Mixed

    public async Task StartGameAsync(User user, int adventureId, DateTime utcNow)
    {
        if (user.CommandProgress != null)
        {
            _context.CommandProgresses.Remove(user.CommandProgress);
        }

        if (user.CurrentGame != null)
        {
            _context.SavedStatuses.Remove(user.CurrentGame.SavedStatus);
            _context.CurrentGames.Remove(user.CurrentGame);
            user.CurrentGame = null;
        }

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

    public async Task ResetGameAsync(CommandProgress commandProgress, SavedStatus savedStatus, DateTime utcNow)
    {
        savedStatus.StoryState = null;
        savedStatus.LastUpdated = utcNow;
        _context.SavedStatuses.Update(savedStatus);
        _context.CommandProgresses.Remove(commandProgress);
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
