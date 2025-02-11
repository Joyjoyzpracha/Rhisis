﻿using Microsoft.EntityFrameworkCore;
using Rhisis.Database.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Rhisis.Database.Repositories.Implementation
{
    /// <summary>
    /// Character repository.
    /// </summary>
    internal sealed class CharacterRepository : RepositoryBase<DbCharacter>, ICharacterRepository
    {
        /// <summary>
        /// Creates and initialize the <see cref="CharacterRepository"/>.
        /// </summary>
        /// <param name="context"></param>
        public CharacterRepository(DbContext context) 
            : base(context)
        {
        }

        /// <inheritdoc />
        public IEnumerable<DbCharacter> GetCharacters(int userId, bool includeDeletedCharacters = false)
        {
            IQueryable<DbCharacter> query = this._context.Set<DbCharacter>().Include(x => x.Items).AsNoTracking();

            query = query.Where(x => x.UserId == userId);

            if (!includeDeletedCharacters)
                query = query.Where(x => !x.IsDeleted);

            return query;
        }

        /// <inheritdoc />
        public DbCharacter GetCharacter(int characterId)
        {
            IQueryable<DbCharacter> query = this._context.Set<DbCharacter>().AsNoTracking();

            return query.FirstOrDefault(x => x.Id == characterId);
        }

        /// <inheritdoc />
        protected override IQueryable<DbCharacter> GetQueryable(DbContext context)
        {
            return base.GetQueryable(context)
                .Include(x => x.User)
                .Include(x => x.Items)
                .Include(x => x.ReceivedMails)
                    .ThenInclude(x => x.Receiver)
                .Include(x => x.ReceivedMails)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.ReceivedMails)
                    .ThenInclude(x => x.Item)
                .Include(x => x.SentMails)
                    .ThenInclude(x => x.Receiver)
                .Include(x => x.SentMails)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.SentMails)
                    .ThenInclude(x => x.Item)
                .Include(x => x.TaskbarShortcuts);
        }
    }
}
