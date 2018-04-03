using Be24Types;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Be24BLogic
{
    public class SystemEventManager
    {



        public void saveEvent(event_type pevent_type, int CurUser, object_type pobj_type_id, int? pobj_id, event_category evCat,  string pdescription)
        {
            saveEvent((int)pevent_type, CurUser, (int)pobj_type_id, pobj_id, (int)evCat,pdescription);
        }



        public  int saveEvent(int pevent_type,int CurUser, int pobj_type_id, int? pobj_id,int evCat,string pdescription)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);
                int objId = 0;
 
                    conn = CoreLogic.PostgresManager.Conn;
             
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_save_event";

                //pevent_type integer, puser_id integer, pobj_type_id integer, pobj_id integer, pdescription text, pevent_category integer
                DbPostgresManager.AddParameter(cmd, "pevent_type", DbType.Int32, ParameterDirection.Input, pevent_type);
                DbPostgresManager.AddParameter(cmd, "puser_id", DbType.Int32, ParameterDirection.Input, CurUser);
                DbPostgresManager.AddParameter(cmd, "pobj_type_id", DbType.Int32, ParameterDirection.Input, pobj_type_id);
                if (pobj_id.HasValue)
                    objId = pobj_id.Value;
                    DbPostgresManager.AddParameter(cmd, "pobj_id", DbType.Int32, ParameterDirection.Input, objId);
                DbPostgresManager.AddParameter(cmd, "pdescription", DbType.String , ParameterDirection.Input, pdescription);
                DbPostgresManager.AddParameter(cmd, "pevent_category", DbType.Int32, ParameterDirection.Input, evCat);
                
                int res = (int)cmd.ExecuteScalar();
                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SystemEventManager.saveEvent " + e.ToString());
                return -1;
              //  throw;
            }
            finally
            {
                if (cmd != null  )
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }

        }

    }
}
