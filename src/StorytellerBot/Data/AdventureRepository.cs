using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StorytellerBot.Models.Data;

namespace StorytellerBot.Data;

public class AdventureRepository
{
    private readonly AdventureContext _context;
    public AdventureRepository(AdventureContext context)
    {
        _context = context;
    }

    #region User

    public async Task<User> GetOrCreateUserAsync(long userId, bool includeSavedGameAdventures = false)
    {
        var user = await GetUserAsync(userId, includeSavedGameAdventures);
        if (user == null)
        {
            user = new User { Id = userId };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    private async Task<User?> GetUserAsync(long userId, bool includeSavedGameAdventures)
    {
        Expression<Func<User, bool>> userById = u => u.Id == userId;
        var query = _context.Users.Include(u => u.SavedGames);
        if (includeSavedGameAdventures)
        {
            return await query.ThenInclude(s => s.Adventure).FirstOrDefaultAsync(userById);
        }
        return await query.FirstOrDefaultAsync(userById);
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

    internal async Task<bool> DeleteSavesForAdventureAsync(int id)
    {
        if (await GetAdventureAsync(id) == null)
        {
            return false;
        }

        var savedStatuses = _context.SavedStatuses.Where(s => s.AdventureId == id);
        _context.SavedStatuses.RemoveRange(savedStatuses);
        await _context.SaveChangesAsync();
        return true;
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

    public async Task<CurrentGame> StartGameAsync(User user, Adventure adventure, DateTime utcNow)
    {
        var newSavedStatus = new SavedStatus
        {
            AdventureId = adventure.Id,
            Adventure = adventure,
            UserId = user.Id,
            User = user,
            LastUpdated = utcNow,
        };
        _context.SavedStatuses.Add(newSavedStatus);
        await _context.SaveChangesAsync();

        var newCurrentGame = new CurrentGame {
            UserId = user.Id,
            SavedStatusId = newSavedStatus.Id,
            User = user,
            SavedStatus = newSavedStatus,
        };
        _context.CurrentGames.Add(newCurrentGame);
        await _context.SaveChangesAsync();
        return newCurrentGame;
    }

    #endregion

    #region Adventure

    internal async Task<List<Adventure>> GetAllAdventuresAsync() =>
        await _context.Adventures.AsNoTracking().ToListAsync();

    internal async Task<bool> AdventureExistsAsync(int adventureId) =>
        await _context.Adventures.AsNoTracking().AnyAsync(a => a.Id == adventureId);

    internal async Task<Adventure> AddAdventureAsync(Adventure newAdventure)
    {
        _context.Adventures.Add(newAdventure);
        await _context.SaveChangesAsync();
        return newAdventure;
    }

    internal async Task<Adventure?> GetAdventureAsync(int id)
    {
        return await _context.Adventures.FirstOrDefaultAsync(a => a.Id == id);
    }

    internal async Task<bool> DeleteAdventureAsync(int id)
    {
        var adventure = await GetAdventureAsync(id);
        if (adventure == null)
        {
            return false;
        }

        _context.Adventures.Remove(adventure);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion
}
