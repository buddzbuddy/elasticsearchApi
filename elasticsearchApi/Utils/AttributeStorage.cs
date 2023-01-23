using elasticsearchApi.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Utils
{
    public class AttributeStorage
    {
        private static readonly Guid UserId = new Guid("05EEF54F-5BFE-4E2B-82C7-6AB6CD59D488");
        private static readonly Guid OrgId = new Guid("B0E44CB2-0E06-4212-87CA-EC11F9D4E18E");
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

        private const string PersonListSql = "SELECT od.Id, od.Name, dt.Name as Type_Name " +
            "FROM Object_Defs AS od " +
            "INNER JOIN Attribute_Defs ad ON ad.Id = od.Id " +
            "INNER JOIN Data_Types dt ON dt.Id = ad.Type_Id " +
            "WHERE od.Parent_Id =@PersonDefId AND (od.Deleted is NULL OR od.Deleted = 0)";

        private const string SelectDocumentAttributeSql =
            "SELECT 1 AS AttrType, " +
            "Value AS Int_Value, " + // 1
            "null AS Currency_Type, " + // 2
            "null AS Text_Type, " + // 3
            "null AS Float_Type, " + // 4
            "null AS Guid_Type, " + // 5
            "null AS Bool_Type, " + // 6
            "null AS Date_Type, Created, Def_Id " + // 7
            "FROM Int_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 2, null, Value, null, null, null, null, null, Created, Def_Id " +
            "FROM Currency_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 3, null, null, Value, null, null, null, null, Created, Def_Id " +
            "FROM Text_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 4, null, null, null, Value, null, null, null, Created, Def_Id " +
            "FROM Float_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 5, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Enum_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 6, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Document_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 8, null, null, null, null, null, Value, null, Created, Def_Id " +
            "FROM Boolean_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 9, null, null, null, null, null, null, Value, Created, Def_Id " +
            "FROM Date_Time_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 12, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Org_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 13, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Doc_State_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 14, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Object_Def_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 15, null, null, File_Name, null, null, null, null, Created, Def_Id " +
            "FROM Image_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n";

        private string connectionString;
        public AttributeStorage(string _connectionString)
        {
            if (_connectionString == null)
                throw new ArgumentNullException("dataContext");

            connectionString = _connectionString;
        }

        public void InsertPerson(Person person)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            var documentId = InsertDocument(connection);
            var personAttributeList = GetPersonAttributeList(connection);
            InsertAttributes(connection, person, documentId, personAttributeList);
        }

        public void UpdateDocument(Person personNew, Guid documentId)
        {
            Person personOld = new Person();
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            var personAttributeList = GetPersonAttributeList(connection);
            var SelectDocumentSQL = BuildSelectPersonSQL(connection, personOld, personAttributeList);

            using SqlCommand command = new(SelectDocumentSQL, connection);
            AddParamWithValue(command, "@DocId", documentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var properties = GetProperties(personNew);
                foreach (PropertyInfo property in properties)
                {
                    var attribute = personAttributeList.Where(x => x.AttributeName.Equals(property.Name)).FirstOrDefault();
                    if (attribute == null) continue;
                    var attributeId = attribute.AttributeId;
                    var objValue = property.GetValue(personNew, null);
                    if (!String.IsNullOrEmpty(property.Name))
                    {
                        var newObjectValue = objValue != null ? objValue.ToString() : "";
                        var oldObjectValue = reader[property.Name].ToString();
                        if (!newObjectValue.Equals(oldObjectValue))
                        {
                            InsertAttribute(connection, documentId, attributeId, property, newObjectValue);
                        }

                    }
                }
            }
        }






        private Guid InsertDocument(SqlConnection connection)
        {
            var newDocumentId = Guid.NewGuid();

            using SqlCommand command = new(SaveDocumentSql, connection);
            
            AddParamWithValue(command, "@Id", newDocumentId);
            AddParamWithValue(command, "@DefId", PersonDefId);
            AddParamWithValue(command, "@Created", DateTime.Now);
            AddParamWithValue(command, "@UserId", UserId);
            AddParamWithValue(command, "@OrgId", OrgId);
            AddParamWithValue(command, "@PositionId", PositionId);
            AddParamWithValue(command, "@Modified", DateTime.Now);
            command.ExecuteNonQuery();
            
            return newDocumentId;
        }



        private void InsertAttributes(SqlConnection connection, Person person, Guid documentId, List<PersonAttribute> personAttributeList)
        {
            var properties = GetProperties(person);
            foreach (PropertyInfo property in properties)
            {
                var objValue = property.GetValue(person, null);
                if (!String.IsNullOrEmpty(property.Name) && (objValue != null))
                {

                    var attribute = personAttributeList.Where(x => x.AttributeName.Equals(property.Name)).FirstOrDefault();
                    if (attribute == null) continue;
                    var attributeId = attribute.AttributeId;
                    var tableName = GetAttributeTableName(property);
                    using SqlCommand command = new(String.Format(SaveAttrSql, tableName), connection);
                    AddParamWithValue(command, "@DocId", documentId);
                    AddParamWithValue(command, "@DefId", attributeId);
                    AddParamWithValue(command, "@Created", DateTime.Now);
                    if (property.GetValue(person, null) != null)
                        AddParamWithValue(command, "@Value", objValue, SqlDbType.NVarChar);
                    else
                        AddParamWithValue(command, "@Value", objValue);
                    AddParamWithValue(command, "@UserId", UserId);
                    command.ExecuteNonQuery();
                }


            }

        }


        private void InsertAttribute(SqlConnection connection, Guid documentId, Guid attributeId, PropertyInfo property, object objValue)
        {

            var tableName = GetAttributeTableName(property);
            using SqlCommand command = new(String.Format(SaveAttrSql, tableName), connection);
            AddParamWithValue(command, "@DocId", documentId);
            AddParamWithValue(command, "@DefId", attributeId);
            AddParamWithValue(command, "@Created", DateTime.Now);
            if (objValue != null)
                AddParamWithValue(command, "@Value", objValue, SqlDbType.NVarChar);
            else
                AddParamWithValue(command, "@Value", objValue);
            AddParamWithValue(command, "@UserId", UserId);
            command.ExecuteNonQuery();
        }

        private PropertyInfo[] GetProperties(Person person)
        {
            Type type = person.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            return type.GetProperties(flags).Where(x => !x.Name.Equals("Id")).ToArray();

        }

        private string BuildSelectPersonSQL(SqlConnection connection, Person person, List<PersonAttribute> attributeList)
        {
            string p = "[Person]";
            int index = 1;
            StringBuilder sqlBuilder1 = new StringBuilder();
            StringBuilder sqlBuilder2 = new StringBuilder();
            StringBuilder sqlBuilder3 = new StringBuilder();
            sqlBuilder1.AppendLine("SELECT TOP 1 ");
            sqlBuilder1.AppendFormat("{0}.[id1], ", p);

            sqlBuilder2.AppendLine(" FROM (SELECT d.Id, ");
            sqlBuilder2.AppendLine("d.Id as [Id1], ");

            sqlBuilder3.AppendLine(" FROM Documents d WITH(NOLOCK) ");

            var properties = GetProperties(person);
            foreach (var property in properties)
            {
                index++;
                var attribute = attributeList.Where(x => x.AttributeName.Equals(property.Name)).FirstOrDefault();
                var tableName = GetTableNameByAttributeType(attribute.AttributeType);
                sqlBuilder1.AppendFormat(" {0}.[{1}], {2}", p, property.Name, Environment.NewLine);
                sqlBuilder2.AppendFormat(" [a{0}].[Value] as [{1}], {2}", index, property.Name, Environment.NewLine);
                sqlBuilder3.AppendFormat(" LEFT OUTER JOIN {0} a{1} WITH(NOLOCK) on (a{1}.Document_Id = d.Id and a{1}.Def_Id = '{2}' and a{1}.Expired = '99991231') {3}", tableName, index, attribute.AttributeId, Environment.NewLine);
            }
            sqlBuilder1.AppendLine("[Person].[tempId] ");
            sqlBuilder2.AppendLine("'0' as [tempId], ");
            sqlBuilder2.AppendLine("d.Last_Modified as [Modified] ");
            sqlBuilder3.AppendLine("WHERE d.Id =@DocId AND ([d].[Deleted] is null OR [d].[Deleted] = 0)");
            sqlBuilder3.AppendFormat(") as {0}", p);

            return sqlBuilder1.ToString() + sqlBuilder2.ToString() + sqlBuilder3.ToString();
        }

        private List<PersonAttribute> GetPersonAttributeList(SqlConnection connection)
        {
            List<PersonAttribute> attributeList = new List<PersonAttribute>();
            using SqlCommand command = new(PersonListSql, connection);
            AddParamWithValue(command, "@PersonDefId", PersonDefId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                PersonAttribute personAttribute = new PersonAttribute { AttributeId = reader.GetGuid(0), AttributeName = reader.GetString(1), AttributeType = reader.GetString(2) };
                attributeList.Add(personAttribute);
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

        private string GetTableNameByAttributeType(string attributeType)
        {
            if (attributeType == "Int")
                return "Int_Attributes";
            else if (attributeType == "Text")
                return "Text_Attributes";
            else if (attributeType == "Float")
                return "Float_Attributes";
            else if (attributeType == "Enum")
                return "Enum_Attributes";
            else if (attributeType == "Boolean")
                return "Boolean_Attributes";
            else if (attributeType == "DateTime")
                return "Date_Time_Attributes";
            else return String.Empty;

        }



    }
}
