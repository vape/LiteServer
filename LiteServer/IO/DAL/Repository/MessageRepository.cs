using LiteServer.IO.DAL.Context;
using LiteServer.IO.DAL.Model;
using System;
using System.Collections.Generic;

namespace LiteServer.IO.DAL.Repository
{
    public interface IMessageRepository : IRepository<Message, long>
    {
        void Send(Guid userUuid, uint groupId, string message, byte attachmentType = 0, long attachmentReference = 0);
        IEnumerable<Message> SelectRange(uint groupId, long offset, long count);
        IEnumerable<Message> SelectMessagesAfterGiven(uint groupId, long offset, long count, long startId);
        IEnumerable<Message> SelectMessagesBeforeGiven(uint groupId, long offset, long count, long startId);
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly BaseContext context;

        public MessageRepository(BaseContext context)
        {
            this.context = context;
        }

        public void Delete(long id)
        {
            context.Db.Delete<Message>(id);
        }

        public void Insert(Message entity)
        {
            context.Db.Insert(entity);
        }

        public Message Select(long id)
        {
            return context.Db.Single<Message>(id);
        }

        public IEnumerable<Message> SelectMessagesBeforeGiven(uint groupId, long offset, long count, long startId)
        {
            return context.Db.Query<Message>("SELECT * FROM messages WHERE group_id=@0 AND id<@3 ORDER BY date DESC LIMIT @1 OFFSET @2", groupId, count, offset, startId);
        }

        public IEnumerable<Message> SelectMessagesAfterGiven(uint groupId, long offset, long count, long startId)
        {
            return context.Db.Query<Message>("SELECT * FROM messages WHERE group_id=@0 AND id>@3 ORDER BY date ASC LIMIT @1 OFFSET @2", groupId, count, offset, startId);
        }

        public IEnumerable<Message> SelectRange(uint groupId, long offset, long count)
        {
            return context.Db.Query<Message>("SELECT * FROM messages WHERE group_id=@0 ORDER BY date DESC LIMIT @1 OFFSET @2", groupId, count, offset);
        }

        public void Send(Guid userUuid, uint groupId, string text, byte attachmentType = 0, long attachmentReference = 0)
        {
            var message = new Message()
            {
                Date = DateTime.UtcNow,
                GroupId = groupId,
                Text = text,
                UserUuid = userUuid,
                AttachmentType = attachmentType,
                AttachmentReference = attachmentReference
            };

            context.Db.Insert(message);
        }
    }
}
