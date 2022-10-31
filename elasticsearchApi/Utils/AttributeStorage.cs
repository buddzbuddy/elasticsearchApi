using elasticsearchApi.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace elasticsearchApi.Utils
{
    public class AttributeStorage
    {
        private static readonly Guid UserId = new Guid ("05EEF54F-5BFE-4E2B-82C7-6AB6CD59D488");
        private static readonly Guid  OrgId = new Guid("B0E44CB2-0E06-4212-87CA-EC11F9D4E18E");
        private static readonly Guid PositionId = new Guid("AAE0CF72-775F-476F-A476-B5DAE32A4C3D");
        private static readonly Guid PersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");
        private const string SaveAttrSql =
           "UPDATE [{0}] WITH(rowlock) SET [Expired] = @Created\n" +
           "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231';\n" +
           "INSERT INTO [{0}] WITH(rowlock) ([Document_Id], [Def_Id], [Created], [Expired], [Value], [UserId])\n" +
           "VALUES(@DocId, @DefId, @Created, '99991231', @Value, @UserId);";
        private const string SaveDocumentSql =
           "UPDATE [Documents] WITH(rowlock) SET [Last_Modified] = @Modified WHERE [Id] = @Id;\n" +
           "IF @@rowcount = 0 BEGIN\n" +
           "INSERT INTO [Documents] WITH(rowlock)\n" +
           " ([Id], [Def_Id], [Created], [UserId], [Organization_Id], [Org_Position_Id], [Last_Modified])\n" +
           "VALUES (@Id, @DefId, @Created, @UserId, @OrgId, @PositionId, @Modified);\n" +
           "END";

        private const string PersonListSql = "SELECT od.Id, od.Name, od.Full_Name, 1, od.Parent_Id " +
            "FROM Object_Defs AS od " +
            "WHERE od.Parent_Id =@PersonDefId AND (od.Deleted is NULL OR od.Deleted = 0)";
        private string connectionString;
        public AttributeStorage(string _connectionString)
        {
            if (_connectionString == null)
                throw new ArgumentNullException("dataContext");

            connectionString = _connectionString;
        }

        public void SavePerson(Person person)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var documentId=InsertDocument(connection);
                var personAttributeList = GetPersonAttributeList(connection);
                InsertAttributes(connection, person, documentId, personAttributeList);
            }
        }

        private Guid InsertDocument(SqlConnection connection)
        {
            var newDocumentId = Guid.NewGuid();
           
                using (SqlCommand command = new SqlCommand(SaveDocumentSql, connection))
                {
                    AddParamWithValue(command, "@Id", newDocumentId);
                    AddParamWithValue(command, "@DefId", PersonDefId);
                    AddParamWithValue(command, "@Created", DateTime.Now);
                    AddParamWithValue(command, "@UserId", UserId);
                    AddParamWithValue(command, "@OrgId", OrgId);
                    AddParamWithValue(command, "@PositionId", PositionId);
                    AddParamWithValue(command, "@Modified", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            return newDocumentId;
        }

        

        private void InsertAttributes(SqlConnection connection,  Person obj, Guid documentId, List<PersonAttribute> personAttributeList)
        {
            Type type = obj.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] properties = type.GetProperties(flags);
            foreach (PropertyInfo property in properties)
            {
                var objValue = property.GetValue(obj, null);
                if (!String.IsNullOrEmpty(property.Name)  && (objValue != null))
                {
                    var tableName = GetAttributeTableName(property);
                    var attribute = personAttributeList.Where(x => x.AttributeName.Equals(property.Name)).FirstOrDefault();
                    if (attribute == null) continue;
                    var attributeId = attribute.AttributeId;
                    using (SqlCommand command = new SqlCommand(String.Format(SaveAttrSql, tableName), connection))
                    {
                        AddParamWithValue(command, "@DocId", documentId);
                        AddParamWithValue(command, "@DefId", attributeId);
                        AddParamWithValue(command, "@Created", DateTime.Now);
                        if (property.GetValue(obj, null) != null)
                            AddParamWithValue(command, "@Value", objValue, SqlDbType.NVarChar);
                        else
                            AddParamWithValue(command, "@Value", objValue);
                        AddParamWithValue(command, "@UserId", UserId);
                        command.ExecuteNonQuery();
                    }
                }
                

            }
           
        }

        private List<PersonAttribute> GetPersonAttributeList(SqlConnection connection)
        {
            List<PersonAttribute> attributeList = new List<PersonAttribute>();
            using (SqlCommand command = new SqlCommand(PersonListSql, connection))
            {
                AddParamWithValue(command, "@PersonDefId", PersonDefId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PersonAttribute personAttribute = new PersonAttribute { AttributeId = reader.GetGuid(0), AttributeName = reader.GetString(1) };
                        attributeList.Add(personAttribute);
                    }
                }
            }
            return attributeList;
        }


        private static void AddParamWithValue(IDbCommand command, string paramName, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = paramName;
            param.Value = value;
            command.Parameters.Add(param);
        }

        private static void AddParamWithValue(IDbCommand command, string paramName, object value, SqlDbType type)
        {
            var param = new SqlParameter(paramName, type) { Value = value };

            command.Parameters.Add(param);
        }

        private string GetAttributeTableName(PropertyInfo property)
        {
            if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Int32))
                return "Int_Attributes";
            else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(String))
                return "Text_Attributes";
            else if (property.PropertyType == typeof(float))
                return "Float_Attributes";
            else if (property.PropertyType == typeof(Enum))
                return "Enum_Attributes";
            else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(Boolean))
                return "Boolean_Attributes";
            else if (property.PropertyType == typeof(DateTime))
                return "Date_Time_Attributes";
            else return String.Empty;
            
        }

      

    }
}
