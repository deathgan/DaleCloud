using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaleCloud.DBUtility
{
    public class SqlBulkCopyHelper
    {

        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();        
        public static int BulkInsert<TModel>(IEnumerable<TModel> source, string tableName)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                //var sw = Stopwatch.StartNew();
                //批量插入数据
                var qty = conn.BulkCopy(source, tableName, 600, 5000);
                //sw.Stop();
                //Debug.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");

                return qty;
            }
        }


        /// <summary>
        /// 批量新增数据（限Excel使用）
        /// </summary>
        /// <param name="dt">DataTable（其中的列名要与数据库表列名一致）</param>
        public static int BatchAdd(DataTable dt, string TableName)
        {
            int rs = 1;
            using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
            {
                sqlConn.Open();
                //SqlTransaction tran = sqlConn.BeginTransaction();//开启事务        
                //SqlBulkCopy sqlbulkcopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.CheckConstraints, tran);
                using (SqlBulkCopy sqlbulkcopy = new SqlBulkCopy(sqlConn))
                {
                    sqlbulkcopy.DestinationTableName = TableName;//数据库中的表名
                    sqlbulkcopy.BatchSize = 5000;
                    try
                    {

                        sqlbulkcopy.WriteToServer(dt);
                        // tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        //tran.Rollback();
                        rs = 0;
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                    return rs;
                }
            }


        }


        /// <summary>
        /// SqlBulkCopy 批量更新数据
        /// </summary>
        /// <param name="dataTable">数据集</param>
        /// <param name="crateTemplateSql">临时表创建字段</param>
        /// <param name="updateSql">更新语句</param>
        public static int BulkUpdateData(DataTable dataTable, string crateTemplateSql, string updateSql)
        {
            int rs = 1;
            using (var sqlConn = new SqlConnection(ConnectionString))
            {
                using (var command = new SqlCommand("", sqlConn))
                {


                    sqlConn.Open();
                    //SqlTransaction tran = sqlConn.BeginTransaction();//开启事务  
                    //数据库并创建一个临时表来保存数据表的数据
                    command.CommandText = String.Format("  CREATE TABLE #TmpTable ({0})", crateTemplateSql);
                    command.ExecuteNonQuery();
                    try
                    {
                        //使用SqlBulkCopy 加载数据到临时表中
                        using (var sqlbulkcopy = new SqlBulkCopy(sqlConn))
                        {
                            foreach (DataColumn dcPrepped in dataTable.Columns)
                            {
                                sqlbulkcopy.ColumnMappings.Add(dcPrepped.ColumnName, dcPrepped.ColumnName);
                            }

                            sqlbulkcopy.BatchSize = 5000;
                            sqlbulkcopy.BulkCopyTimeout = 3600;
                            sqlbulkcopy.DestinationTableName = "#TmpTable";
                            sqlbulkcopy.WriteToServer(dataTable);
                            sqlbulkcopy.Close();
                        }

                        // 执行Command命令 使用临时表的数据去更新目标表中的数据  然后删除临时表
                        command.CommandTimeout = 1800;
                        command.CommandText = updateSql;
                        // tran.Commit();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {

                        //   tran.Rollback();
                        rs = 0;
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }

                return rs;
            }


        }







    }
}
