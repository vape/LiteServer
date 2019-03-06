using LiteServer.IO.DAL.Context;
using LiteServer.IO.DAL.Model;
using LiteServer.Utils;
using System;
using System.Linq;

namespace LiteServer.IO.DAL.Repository
{
    public interface ITokenRepository : IRepository<Token, Guid>
    {
        Token CreateToken(Guid userUuid, TimeSpan duration);
        Token TrySelect(Guid id);
    }

    public class TokenRepository : ITokenRepository
    {
        private BaseContext context;

        public TokenRepository(BaseContext context)
        {
            this.context = context;
        }

        public Token CreateToken(Guid userUuid, TimeSpan duration)
        {
            var value = Guid.NewGuid();
            var token = new Token()
            {
                ExpireDate = DateTime.UtcNow + duration,
                UserUuid = userUuid,
                Value = value
            };

            context.Db.Insert(token);
            return token;
        }

        public void Delete(Guid id)
        {
            context.Db.Delete<Token>(id.ToBytes());
        }

        public void Insert(Token entity)
        {
            context.Db.Insert(entity);
        }

        public Token Select(Guid id)
        {
            return context.Db.Single<Token>(id.ToBytes());
        }

        public Token TrySelect(Guid id)
        {
            return context.Db.Query<Token>("SELECT * FROM token WHERE value=@0", id.ToBytes()).FirstOrDefault();
        }
    }
}
