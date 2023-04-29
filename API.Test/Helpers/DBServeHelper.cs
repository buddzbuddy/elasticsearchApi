using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Helpers
{
    public static class DBServeHelper
    {
        public static void RestoreDatabaseToLastPoint()
        {
            var connectionString = "Server=192.168.2.150,14331;Database=master;User Id=sa;Password=P@ssword123;Encrypt=False";
            SqlConnection connection = new(connectionString);
            connection.Open();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = CALL_RESTORE_PROCEDURE_T_SQL;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
        }
        private const string CALL_RESTORE_PROCEDURE_T_SQL = @"
USE master
EXECUTE [dbo].[RestoreNrszTest] 
GO";
        private const string KILL_T_SQL = @"
DECLARE @kill varchar(8000) = '';  
SELECT  @kill = @kill + 'kill ' + CONVERT(varchar(5), request_session_id) + ';'
FROM sys.dm_tran_locks 
WHERE resource_database_id = DB_ID('nrsz-test')
EXEC(@kill);
";
        private const string RESTORE_T_SQL = @"
DECLARE @kill varchar(8000) = '';  
SELECT  @kill = @kill + 'kill ' + CONVERT(varchar(5), request_session_id) + ';'
FROM sys.dm_tran_locks 
WHERE resource_database_id = DB_ID('nrsz-test')
EXEC(@kill);

RESTORE DATABASE [nrsz-test] FROM  DISK = N'/var/opt/mssql/data/nrsz-test.bak'
WITH REPLACE
";
    }
}
