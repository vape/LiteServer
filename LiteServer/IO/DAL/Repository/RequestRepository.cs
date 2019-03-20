using LiteServer.IO.DAL.Context;
using LiteServer.IO.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteServer.IO.DAL.Repository
{
    public interface IRequestRepository : IRepository<Request, uint>
    {
        bool FillRequest(uint id);
    }

    public class RequestRepository : IRequestRepository
    {
        private BaseContext context;

        public RequestRepository(BaseContext context)
        {
            this.context = context;
        }

        public void Delete(uint id)
        {
            context.Db.Delete<Request>(id);
        }

        public void Insert(Request entity)
        {
            context.Db.Insert(entity);
        }

        public Request Select(uint id)
        {
            return context.Db.Single<Request>(id);
        }

        public bool FillRequest(uint id)
        {
            return context.Db.ExecuteScalar<bool>("SELECT fill_request(@0)", id);
        }
    }
}
