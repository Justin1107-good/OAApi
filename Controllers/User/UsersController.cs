using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using MySqlConnector;
using SynergyCommon.Context;
using SynergyCore.user;
using SynergyEntity;
using SynergyEntity.test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OASystemSynergy.Controllers.User
{

    [Route("api/Users")]
    [ApiController]
    [EnableCors("any")]

    public class UsersController : ControllerBase
    {
        /// <summary>
        /// 本地认证评估表建表SQL
        /// </summary>
        private const string CreateTemplateSql = @"id int,NameU varchar(50),PWD varchar(50)";
        //create table UserInfo11 (id int,NameU varchar(50),PWD varchar(50));
        /// <summary>
        /// 本地认证评估更新SQL 这里采用的merge语言更新语句 你也可以使用 sql update 语句
        /// </summary>
        private const string UpdateSql = "update UserInfo11, TmpTable set UserInfo11.NameU=TmpTable.NameU where UserInfo11.id=TmpTable.id;";
        //@"/*Merge into UserInfo11 AS T Using TmpTable AS S ON T.id = S.id WHEN MATCHED THEN UPDATE SET T.NameU=S.NameU;*/";
        private readonly IUserFactory _userFactory;
        public UsersController(IUserFactory userFactory)
        {
            _userFactory = userFactory;
        }
        // GET: api/<UsersController>

        [Route("userList")]
        [HttpGet]
        //[EnableCors("any")]
        public List<tb_User> Get()
        {
            var list = _userFactory.UserList();
            // string json = JsonConvert.SerializeObject(list);
            return list;
        }

        // GET api/<UsersController>/5
        [HttpGet("Login")]
        public int Get(string UserName, string PassWord)
        {

            int flag = 0;
            try
            {
                if (_userFactory.Login(UserName, PassWord) > 0)
                {
                    flag = 1;
                }
                else
                {
                    return flag;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally { }
            return flag;
        }

        // POST api/<UsersController>
        [Route("Add")]
        [HttpPost]
        public int Post([FromBody] tb_User user)
        {
            int h = _userFactory.Add(user);
            if (h > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        [Route("MultAdd")]
        [HttpPost]
        public int MultAddUser(List<tb_User> users)
        {
            int h = _userFactory.Add(users);
            if (h > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("delete")]
        public int Delete(int id)
        {
            int h = _userFactory.Remove(id);
            if (h > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        [HttpDelete("multDelete")]
        public int MultDelect(string idStr)
        {
            string[] id = idStr.Split(',');
            return _userFactory.Remove(id);
        }
        [Route("UserUpdate")]
        [HttpPost]
        public bool UodateList(List<tb_User> user)
        {
            int h = _userFactory.GetUpdate(user);
            //MqlBulkUpdateData<UserInfo11>(user, CreateTemplateSql, UpdateSql);
            return true;
        }

        public static DataTable ToDataTable(List<UserInfo11> list)
        {
            DataTable table = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    Type pt = pi.PropertyType;
                    if ((pt.IsGenericType) && (pt.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        pt = pt.GetGenericArguments()[0];
                    }
                    table.Columns.Add(new DataColumn(pi.Name, pt));
                }

                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    table.LoadDataRow(array, true);
                }
            }
            return table;
        }

        public static DataTable ToDataTable<T>(List<tb_User> list)
        {
            DataTable table = new DataTable();
            //创建列头
            PropertyInfo[] propertys = typeof(T).GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                Type pt = pi.PropertyType;
                if ((pt.IsGenericType) && (pt.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    pt = pt.GetGenericArguments()[0];
                }
                table.Columns.Add(new DataColumn(pi.Name, pt));
            }
            //创建数据行
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    table.LoadDataRow(array, true);
                }
            }
            return table;
        }

        public bool Update<T>(T model)
        {
            T t = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] plist = typeof(T).GetProperties();
            SqlParameter[] parameters = new SqlParameter[plist.Length];
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update " + typeof(T).Name + " set ");
            for (int i = 1; i < plist.Length; i++)
            //这里i从1开始是因为一般plist[0]是主键id，主键是不能修改的
            {
                strSql.Append("" + plist[i].Name + "=" + plist[i].Name + ",");
            }
            strSql.Remove(strSql.Length - 1, 1);
            strSql.Append(" where " + plist[0].Name + "=" + plist[0].Name + "");
            for (int i = 0; i < plist.Length; i++)
            {
                parameters[i] = new SqlParameter("" + plist[i].Name + "", plist[i].GetValue(t));
            }
            int rows = ExecuteSql(strSql.ToString());
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int ExecuteSql(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection("Data Source=192.168.220.128;Initial Catalog=NetCoreDemo;User ID=sa;pwd=Skycore2021"))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        #region sqlserver


        public static void BulkUpdateData<T>(List<T> list, string crateTemplateSql, string updateSql)
        {
            var dataTable = ConvertToDataTable(list);

            using (var conn = new SqlConnection("Data Source=192.168.220.128;Initial Catalog=NetCoreDemo;User ID=sa;pwd=Skycore2021"))
            {
                using (var command = new SqlCommand("", conn))
                {
                    try
                    {
                        conn.Open();
                        //数据库并创建一个临时表来保存数据表的数据
                        command.CommandText = $"  CREATE TABLE #TmpTable ({crateTemplateSql})";
                        command.ExecuteNonQuery();

                        //使用SqlBulkCopy 加载数据到临时表中
                        using (var bulkCopy = new SqlBulkCopy(conn))
                        {
                            foreach (DataColumn dcPrepped in dataTable.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(dcPrepped.ColumnName, dcPrepped.ColumnName);
                            }

                            bulkCopy.BulkCopyTimeout = 660;
                            bulkCopy.DestinationTableName = "#TmpTable";
                            bulkCopy.WriteToServer(dataTable);
                            bulkCopy.Close();
                        }

                        // 执行Command命令 使用临时表的数据去更新目标表中的数据  然后删除临时表
                        command.CommandTimeout = 300;
                        command.CommandText = updateSql;
                        command.ExecuteNonQuery();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)
            {
                var row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        #region mysql
        public static async Task MqlBulkUpdateData<T>(List<T> list, string crateTemplateSql, string updateSql)
        {
            var dataTable = ConvertToDataTable(list);

            using (var conn = new MySqlConnector.MySqlConnection("server = 127.0.0.1; uid = root; pwd = root; database = testdb;AllowLoadLocalInfile=true "))
            {

                using (var command = new MySqlConnector.MySqlCommand("", conn))
                {
                    try
                    {
                        conn.Open();
                        //数据库并创建一个临时表来保存数据表的数据
                        command.CommandText = $"CREATE TABLE TmpTable ({CreateTemplateSql});";
                        command.ExecuteNonQuery();

                        #region mysqlbulkcopy  
                        //MySqlBulkCopy bulkCopy = new MySqlBulkCopy(conn);
                        ////使用SqlBulkCopy 加载数据到临时表中                        

                        ////foreach (DataColumn dcPrepped in dataTable.Columns)
                        ////{
                        ////    bulkCopy.ColumnMappings.Add(dcPrepped.ColumnName);
                        ////}
                        //bulkCopy.DestinationTableName = "TmpTable";
                        //var result = bulkCopy.WriteToServer(dataTable);
                        //
                        //
                        // open the connection
                        //using var connection = new MySqlConnection("...;AllowLoadLocalInfile=True");
                        //await connection.OpenAsync();

                        // bulk copy the data
                        var bulkCopy = new MySqlBulkCopy(conn);

                        MySqlBulkCopyColumnMapping cmp = null;
                        foreach (DataColumn dcPrepped in dataTable.Columns)
                        {
                            cmp = new MySqlBulkCopyColumnMapping
                            {
                                DestinationColumn = dcPrepped.ColumnName,
                                Expression = dcPrepped.ColumnName
                            };

                            //bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping
                            //{

                            //    DestinationColumn = dcPrepped.ColumnName,



                            //});

                        }
                        bulkCopy.ColumnMappings.Add(cmp);
                        bulkCopy.DestinationTableName = "TmpTable";
                        await bulkCopy.WriteToServerAsync(dataTable);
                        // 执行Command命令 使用临时表的数据去更新目标表中的数据  然后删除临时表
                        command.CommandTimeout = 300;
                        command.CommandText = updateSql;
                        command.ExecuteNonQuery();

                        #endregion

                        #region MySqlBulkLoader    
                        //MySqlBulkLoader bl = new MySqlBulkLoader(conn);
                        //bl.Local = true;
                        //bl.TableName = "TmpTable";
                        //foreach (DataColumn dcPrepped in dataTable.Columns)
                        //{
                        //    bl.Columns.Add(dcPrepped.ColumnName);
                        //}
                        //bl.NumberOfLinesToSkip = 3;
                        //int result = bl.Columns.Count;

                        //if (result != 0)
                        //{
                        //    // 执行Command命令 使用临时表的数据去更新目标表中的数据  然后删除临时表
                        //    command.CommandTimeout = 300;
                        //    command.CommandText = updateSql;
                        //    command.ExecuteNonQuery();
                        //}
                        #endregion
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion                     



    }
}
