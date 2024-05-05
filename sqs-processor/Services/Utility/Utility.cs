using Microsoft.EntityFrameworkCore;
using sqs_processor.DbContexts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace sqs_processor.Services.Utility
{
    public enum SQLCallType
    {
        Update,
        Insert
    }

    public class FieldValue
    {
        public FieldValue(int fldInsert)
        {
            fieldInsert = fldInsert;
        }
        public string field { get; set; }
        public string value { get; set; }
        public int fieldInsert { get; set; }
    }
    public class Utility : IUtility

    {

        public void UpdateRecords(IEnumerable<object> records, SecuritiesLibraryContext _context)
        {
            StringBuilder fullStringSqlCall = new StringBuilder();
            int loopCount = 0;
            foreach (var record in records)
            {
                Type type = record.GetType();
                var tableName = GetTableName(record);
                if (tableName == "")
                {
                    tableName = type.Name;
                }
                string whereStatement = "";
                string values = "";

                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                PropertyInfo[] properties = type.GetProperties(flags);
                var fieldInsert = 0;
                values += "";
                foreach (PropertyInfo property in properties)
                {
                    string fieldName = property.Name;

                    // if (fieldName.ToUpper() == "ID")
                    //{
                    //  continue;
                    // }
                    var custAttributes = property.GetCustomAttributes();
                    bool isKey = false;
                    foreach (var custAttribute in custAttributes)
                    {
                        if (custAttribute.GetType().Name == "KeyAttribute")
                        {
                            //This Property is the key
                            isKey = true;
                        }

                    }
                    if (isKey)
                    {
                        var propertyValue = property.GetValue(record, null);
                        int recValueInt = (Int32)propertyValue;
                        whereStatement += " WHERE " + fieldName + " = " + recValueInt.ToString() + ";";

                        continue;
                    }

                    FieldValue fieldValue = GetFieldAndValue(record, property, fieldInsert, (int)SQLCallType.Update);
                    values += fieldValue.value;
                    fieldInsert = fieldValue.fieldInsert;



                }





                fullStringSqlCall.Append("UPDATE " + tableName + " SET " + values + whereStatement);

                loopCount += 1;
                if (loopCount > 300)
                {
                    _context.Database.ExecuteSqlRaw(fullStringSqlCall.ToString());
                    loopCount = 0;
                    fullStringSqlCall = new StringBuilder();
                }

            }
            if (loopCount > 0)
            {
                _context.Database.ExecuteSqlRaw(fullStringSqlCall.ToString());

            }



        }


        public void AddRecords(IEnumerable<object> records, SecuritiesLibraryContext _context)
        {

            Dictionary<string, StringBuilder> fullStringSqlCalls = new Dictionary<string, StringBuilder>();

            
            int loopCount = 0;
            foreach (var record in records)
            {
                Type type = record.GetType();


                var tableName = GetTableName(record);
                if (tableName == "")
                {
                    tableName = type.Name;
                }

                string fields = "";
                string values = "";

                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                PropertyInfo[] properties = type.GetProperties(flags);
                var fieldInsert = 0;
                fields += "(";
                values += "(";
                foreach (PropertyInfo property in properties)
                {
                    string fieldName = property.Name;
                    var info = property.GetValue(record, null);
                    // if (fieldName.ToUpper() == "ID")
                    //{
                    //  continue;
                    // }
                    var custAttributes = property.GetCustomAttributes();
                    bool isKey = false;
                    foreach (var custAttribute in custAttributes)
                    {
                        if (custAttribute.GetType().Name == "KeyAttribute")
                        {
                            //This Property is the key
                            isKey = true;
                        }

                    }
                    // fields +=
                    if (isKey)
                    {
                        continue;
                    }
                    FieldValue fieldValue = GetFieldAndValue(record, property, fieldInsert, (int)SQLCallType.Insert);
                    fields += fieldValue.field;
                    values += fieldValue.value;
                    fieldInsert = fieldValue.fieldInsert;

                }

                fields += ")";
                values += ")";

                if (!fullStringSqlCalls.ContainsKey(fields))
                {
                    fullStringSqlCalls[fields] = new StringBuilder();
                    fullStringSqlCalls[fields].Append("INSERT INTO " + tableName + fields + " VALUES ");
                }
                else {
                    fullStringSqlCalls[fields].Append(",");
                }
                fullStringSqlCalls[fields].Append(values);

                loopCount += 1;
                if (loopCount > 400)
                {
                    List<string> stringCalls = fullStringSqlCalls.Keys.ToList();
                    foreach (string stringCall in stringCalls)
                    {
                        fullStringSqlCalls[stringCall].Append(";");
                        string sqlCallinfo = fullStringSqlCalls[stringCall].ToString();
                        _context.Database.ExecuteSqlRaw(sqlCallinfo);

                    }
                 
                    loopCount = 0;
                    fullStringSqlCalls = new Dictionary<string, StringBuilder>();
                }

            }
            if (loopCount > 0)
            {
                List<string> stringCalls = fullStringSqlCalls.Keys.ToList();
                foreach (string stringCall in stringCalls)
                {
                    fullStringSqlCalls[stringCall].Append(";");
                    string sqlCallinfo = fullStringSqlCalls[stringCall].ToString();
                    _context.Database.ExecuteSqlRaw(sqlCallinfo);

                }
            }

        }


        public static string GetTableName(object c)
        {
            string displayName = "";
            DisplayAttribute dp = c.GetType().GetCustomAttributes().Cast<DisplayAttribute>().SingleOrDefault();
            if (dp != null)
            {
                displayName = dp.Name;
            }
            return displayName;
        }

        private FieldValue GetFieldAndValue(Object record, PropertyInfo property, int fieldInsert, int sqlCallType)
        {
            FieldValue fieldValue = new FieldValue(fieldInsert);
            var propertyType = property.PropertyType.Name;
            var propertyValue = property.GetValue(record, null);
            string fieldName = property.Name;
            string valueRec = "";
            switch (propertyType)
            {
                case "Int32":
                    int recValueInt = (Int32)propertyValue;
                    valueRec = recValueInt.ToString();
                    break;
                case "String":
                    string recValueStr = (string)propertyValue;
                    if (recValueStr == null)
                    {
                        return fieldValue;
                    }
                    valueRec = "'" + recValueStr.Replace("'", "''") + "'";
                    break;
                case "DateTime":
                    string recValuestr = ((DateTime)propertyValue).ToString("yyyy-MM-dd HH:mm:ss");
                    valueRec = "'" + recValuestr + "'";
                    break;
                case "Decimal":
                    decimal recValueDec = (decimal)propertyValue;
                    valueRec = recValueDec.ToString();

                    break;
                case "Boolean":
                    bool recValueBool = (bool)propertyValue;
                    valueRec = recValueBool ? "1" : "0";
                    break;
                case "Nullable`1":
                    var recValueNull = propertyValue;
                    if (recValueNull == null)
                    {
                        return fieldValue;
                    }

                    var genericTypes = property.PropertyType.GenericTypeArguments;
                    string nullableTypeName = "";
                    foreach (var genericType in genericTypes)
                    {
                        nullableTypeName = genericType.Name;
                    }
                    if (nullableTypeName != string.Empty)
                    {
                        return NullGetFieldAndValue(fieldValue, nullableTypeName, propertyValue, fieldName, sqlCallType);
                    }
                    break;
            }



            switch (sqlCallType)
            {
                case (int)SQLCallType.Update:
                    if (fieldValue.fieldInsert > 0)
                    {
                        fieldValue.value += ",";
                    }
                    fieldValue.value += fieldName + " = " + valueRec;
                    fieldValue.fieldInsert += 1;
                    break;
                case (int)SQLCallType.Insert:
                    if (fieldValue.fieldInsert > 0)
                    {
                        fieldValue.field += ",";
                        fieldValue.value += ",";
                    }
                    fieldValue.field += fieldName;
                    fieldValue.value += valueRec;
                    fieldValue.fieldInsert += 1;
                    break;
            }



            return fieldValue;
        }


        public FieldValue NullGetFieldAndValue(FieldValue fieldValue, string nullableTypeName, object propertyValue, string fieldName, int sqlCallType)
        {
            string valueRec = "";
            switch (nullableTypeName)
            {

                case "Int32":
                    int recValueInt = (Int32)propertyValue;
                    valueRec = recValueInt.ToString();
                    break;
                case "String":
                    string recValueStr = (string)propertyValue;
                    if (recValueStr == null)
                    {
                        return fieldValue;
                    }
                    valueRec = "'" + recValueStr.Replace("'", "''") + "'";
                    break;
                case "DateTime":

                    string recValuestr = ((DateTime)propertyValue).ToString("yyyy-MM-dd HH:mm:ss");
                    valueRec = "'" + recValuestr + "'";
                    break;
                case "Decimal":
                    decimal recValueDec = (decimal)propertyValue;
                    valueRec = recValueDec.ToString();
                    break;
                case "Boolean":
                    bool recValueBool = (bool)propertyValue;
                    valueRec = recValueBool ? "1" : "0";
                    break;

            }

            switch (sqlCallType)
            {
                case (int)SQLCallType.Update:
                    if (fieldValue.fieldInsert > 0)
                    {
                        fieldValue.value += ",";
                    }
                    fieldValue.value += fieldName + " = " + valueRec;
                    fieldValue.fieldInsert += 1;
                    break;
                case (int)SQLCallType.Insert:
                    if (fieldValue.fieldInsert > 0)
                    {
                        fieldValue.field += ",";
                        fieldValue.value += ",";
                    }
                    fieldValue.field += fieldName;
                    fieldValue.value += valueRec;
                    fieldValue.fieldInsert += 1;
                    break;
            }

            return fieldValue;
        }
    }
}
