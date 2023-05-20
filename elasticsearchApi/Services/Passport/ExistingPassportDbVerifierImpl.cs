using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Infrastructure;
using SqlKata.Execution;
using System;
using System.Data.SqlClient;

namespace elasticsearchApi.Services.Passport
{
    public class ExistingPassportDbVerifierImpl : IExistingPassportVerifier
    {
        private readonly QueryFactory _queryFactory;
        private readonly AppTransaction _appTransaction;
        public ExistingPassportDbVerifierImpl(QueryFactory queryFactory, AppTransaction appTransaction)
        {
            _queryFactory = queryFactory;
            _appTransaction = appTransaction;
        }
        readonly object lockObj = new ();
        public void CheckExistingPassportByNo(string passportNo, int? excludePersonId = null)
        {
            string sql = string.Format(CheckExistingPassportSql, passportNo);
            if (excludePersonId != null && excludePersonId > 0)
                sql = string.Format(CheckExistingPassportWithExcludedIdSql, passportNo, excludePersonId);
            var connectionString = "Server=192.168.2.150,14331;Database=nrsz-test;User Id=sa;Password=P@ssword123;Encrypt=False";
            lock (lockObj)
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();
                //connection.EnlistTransaction((System.Transactions.Transaction?)_appTransaction.Transaction);
                using SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var IIN = reader.IsDBNull(0) ? "пин отсутствует" : reader.GetString(0);
                    var Last_Name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var First_Name = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var Middle_Name = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var msg = $"Найден дубликат по паспорту! Данный номер ({passportNo}) паспорта принадлежит существующему гражданину ПИН: {IIN}, ФИО: {Last_Name} {First_Name} {Middle_Name}, Номер паспорта: {passportNo}";
                    connection.Close();
                    connection.Dispose();
                    throw new PassportDuplicateException(msg);
                }
            }
        }

        private const string CheckExistingPassportSql = @"
SELECT [IIN]
      ,[Last_Name]
      ,[First_Name]
      ,[Middle_Name]
  FROM [nrsz-test].[dbo].[Persons]
where [deleted] = 0 and [PassportNo] = N'{0}'
";
        private const string CheckExistingPassportWithExcludedIdSql = @"
SELECT [Id]
      ,[IIN]
      ,[SIN]
      ,[Last_Name]
      ,[First_Name]
      ,[Middle_Name]
      ,[Date_of_Birth]
      ,[Sex]
      ,[PassportType]
      ,[PassportSeries]
      ,[PassportNo]
      ,[Date_of_Issue]
      ,[Issuing_Authority]
      ,[FamilyState]
      ,[CreatedAt]
      ,[ModifiedAt]
      ,[deleted]
FROM [nrsz-test].[dbo].[Persons]
where [deleted] = 0 and [PassportNo] = N'{0}' and Id <> {1}
";
    }
}
