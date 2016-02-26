using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MaximoDBPopulator {
    public class Program {
        private static void Main() {
            //var connString = "Data Source=10.50.100.51;Initial Catalog=FS76DEV;User Id=MAXIMO;password=MAXIMO;";
            //using (var connection = new SqlConnection(connString)) {

            //    var customizations = new Dictionary<string, string>();
            //    customizations.Add("description", "Populate test emesquita");
            //    customizations.Add("wonum", "-" + DateTime.Now.Ticks);
            //    customizations.Add("workorderid", "-" + DateTime.Now.Ticks);

            //    var exclusions = new List<string>();
            //    //exclusions.Add("wonum");
            //    //exclusions.Add("workorderid");
            //    exclusions.Add("rowstamp");

            //    // Create the Command and Parameter objects.
            //    var command = new SqlCommand("select TOP(1) * from workorder", connection);

            //    //var adapter = new SqlDataAdapter();
            //    var columns = new List<string>();
            //    var parameters = new List<string>();

            //    // Open the connection in a try/catch block. 
            //    // Create and execute the DataReader, writing the result
            //    // set to the console window.
            //    try {
            //        connection.Open();
            //        var reader = command.ExecuteReader();

            //        if (!reader.Read()) {
            //            Console.WriteLine("No base result found.");
            //            Console.ReadLine();
            //            return;
            //        }

            //        var insert = new SqlCommand();
            //        insert.Connection = connection;
            //        for (var i = 0; i < reader.FieldCount; i++) {
            //            var columnName = reader.GetName(i);
            //            var val = reader[i];
            //            var type = GetSqlType(reader.GetFieldType(i));

            //            if (customizations.Keys.Any(c => c.Equals(columnName))) {
            //                val = customizations[columnName];
            //            }

            //            if (string.IsNullOrEmpty(val?.ToString()) || exclusions.Any(c => c.Equals(columnName))) {
            //                continue;
            //            }

            //            columns.Add(columnName);
            //            var parameter = "@" + columnName;
            //            var sqlParameter = new SqlParameter(parameter, type) { Value = val };
            //            insert.Parameters.Add(sqlParameter);
            //            parameters.Add(parameter);
            //        }
            //        reader.Close();


            //        var columnsString = string.Join(", ", columns);
            //        var parametersString = string.Join(", ", parameters);
            //        insert.CommandText =
            //            $"INSERT INTO workorder ({columnsString}) VALUES ({parametersString})";


            //        Console.WriteLine(insert.CommandText);

            //        //insert.ExecuteNonQuery();
            //    } catch (Exception ex) {
            //        Console.WriteLine(ex.Message);
            //    }
            //    
            //}

            try {

                var config = new Configuration();
                config.Load();
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Get the equivalent SQL data type of the given type.
        /// </summary>
        /// <param name="type">Type to get the SQL type equivalent of</param>
        public static SqlDbType GetSqlType(Type type) {
            if (type == typeof(string)) {
                return SqlDbType.NVarChar;
            }

            if (type == typeof(byte[])) {
                return SqlDbType.VarBinary;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                type = Nullable.GetUnderlyingType(type);
            }

            var param = new SqlParameter("", Activator.CreateInstance(type));
            return param.SqlDbType;
        }
    }
}
