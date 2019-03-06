using System;

namespace LiteServer.IO.DAL.Context
{
    public class BaseContext
    {
        public PetaPoco.Database Db;

        public BaseContext(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            Db = new PetaPoco.Database(connectionString, new PetaPoco.Providers.MySqlDatabaseProvider());
        }
    }
}
