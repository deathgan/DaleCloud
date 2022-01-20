using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaleCloud.DBUtility
{
    public static class SqlConnectionExtension
    {
        /// <summary>
        /// 批量复制
        /// </summary>
        /// <typeparam name="TModel">插入的模型对象</typeparam>
        /// <param name="source">需要批量插入的数据源</param>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="tableName">插入表名称【为NULL默认为实体名称】</param>
        /// <param name="bulkCopyTimeout">插入超时时间</param>
        /// <param name="batchSize">写入数据库一批数量【如果为0代表全部一次性插入】最合适数量【这取决于您的环境，尤其是行数和网络延迟。就个人而言，我将从BatchSize属性设置为1000行开始，然后看看其性能如何。如果可行，那么我将使行数加倍（例如增加到2000、4000等），直到性能下降或超时。否则，如果超时发生在1000，那么我将行数减少一半（例如500），直到它起作用为止。】</param>
        /// <param name="options">批量复制参数</param>
        /// <param name="externalTransaction">执行的事务对象</param>
        /// <returns>插入数量</returns>
        public static int BulkCopy<TModel>(this SqlConnection connection,
            IEnumerable<TModel> source,
            string tableName = null,
            int bulkCopyTimeout = 30,
            int batchSize = 0,
            SqlBulkCopyOptions options = SqlBulkCopyOptions.Default,
            SqlTransaction externalTransaction = null)
        {
            //创建读取器
            using (var reader = new EnumerableReader<TModel>(source))
            {
                //创建批量插入对象
                using (var copy = new SqlBulkCopy(connection, options, externalTransaction))
                {
                    //插入的表
                    copy.DestinationTableName = tableName ?? typeof(TModel).Name;
                    //写入数据库一批数量
                    copy.BatchSize = batchSize;
                    //超时时间
                    copy.BulkCopyTimeout = bulkCopyTimeout;
                    //创建字段映射【如果没有此字段映射会导致数据填错位置，如果类型不对还会导致报错】【因为：没有此字段映射默认是按照列序号对应插入的】
                    foreach (var column in ModelToDataTable<TModel>.Columns)
                    {
                        //创建字段映射
                        copy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    //将数据批量写入数据库
                    copy.WriteToServer(reader);
                    //返回插入数据数量
                    return reader.Depth;
                }
            }
        }

        /// <summary>
        /// 批量复制-异步
        /// </summary>
        /// <typeparam name="TModel">插入的模型对象</typeparam>
        /// <param name="source">需要批量插入的数据源</param>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="tableName">插入表名称【为NULL默认为实体名称】</param>
        /// <param name="bulkCopyTimeout">插入超时时间</param>
        /// <param name="batchSize">写入数据库一批数量【如果为0代表全部一次性插入】最合适数量【这取决于您的环境，尤其是行数和网络延迟。就个人而言，我将从BatchSize属性设置为1000行开始，然后看看其性能如何。如果可行，那么我将使行数加倍（例如增加到2000、4000等），直到性能下降或超时。否则，如果超时发生在1000，那么我将行数减少一半（例如500），直到它起作用为止。】</param>
        /// <param name="options">批量复制参数</param>
        /// <param name="externalTransaction">执行的事务对象</param>
        /// <returns>插入数量</returns>
        public static async Task<int> BulkCopyAsync<TModel>(this SqlConnection connection,
            IEnumerable<TModel> source,
            string tableName = null,
            int bulkCopyTimeout = 30,
            int batchSize = 0,
            SqlBulkCopyOptions options = SqlBulkCopyOptions.Default,
            SqlTransaction externalTransaction = null)
        {
            //创建读取器
            using (var reader = new EnumerableReader<TModel>(source))
            {
                //创建批量插入对象
                using (var copy = new SqlBulkCopy(connection, options, externalTransaction))
                {
                    //插入的表
                    copy.DestinationTableName = tableName ?? typeof(TModel).Name;
                    //写入数据库一批数量
                    copy.BatchSize = batchSize;
                    //超时时间
                    copy.BulkCopyTimeout = bulkCopyTimeout;
                    //创建字段映射【如果没有此字段映射会导致数据填错位置，如果类型不对还会导致报错】【因为：没有此字段映射默认是按照列序号对应插入的】
                    foreach (var column in ModelToDataTable<TModel>.Columns)
                    {
                        //创建字段映射
                        copy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    //将数据批量写入数据库
                    await copy.WriteToServerAsync(reader);
                    //返回插入数据数量
                    return reader.Depth;
                }
            }
        }
    }

    /// <summary>
    /// 迭代器数据读取器
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    public class EnumerableReader<TModel> : IDataReader
    {
        /// <summary>
        /// 实例化迭代器读取对象
        /// </summary>
        /// <param name="source">模型源</param>
        public EnumerableReader(IEnumerable<TModel> source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _enumerable = source.GetEnumerator();
        }

        private readonly IEnumerable<TModel> _source;
        private readonly IEnumerator<TModel> _enumerable;
        private object[] _currentDataRow = new object[] { };
        private int _depth;
        private bool _release;

        public void Dispose()
        {
            _release = true;
            _enumerable.Dispose();
        }

        public int GetValues(object[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var length = Math.Min(_currentDataRow.Length, values.Length);
            Array.Copy(_currentDataRow, values, length);
            return length;
        }

        public int GetOrdinal(string name)
        {
            for (int i = 0; i < ModelToDataTable<TModel>.Columns.Count; i++)
            {
                if (ModelToDataTable<TModel>.Columns[i].ColumnName == name) return i;
            }

            return -1;
        }

        public long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            if (dataIndex < 0) throw new Exception($"起始下标不能小于0！");
            if (bufferIndex < 0) throw new Exception("目标缓冲区起始下标不能小于0！");
            if (length < 0) throw new Exception("读取长度不能小于0！");
            var numArray = (byte[])GetValue(ordinal);
            if (buffer == null) return numArray.Length;
            if (buffer.Length <= bufferIndex) throw new Exception("目标缓冲区起始下标不能大于目标缓冲区范围！");
            var freeLength = Math.Min(numArray.Length - bufferIndex, length);
            if (freeLength <= 0) return 0;
            Array.Copy(numArray, dataIndex, buffer, bufferIndex, length);
            return freeLength;
        }

        public long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            if (dataIndex < 0) throw new Exception($"起始下标不能小于0！");
            if (bufferIndex < 0) throw new Exception("目标缓冲区起始下标不能小于0！");
            if (length < 0) throw new Exception("读取长度不能小于0！");
            var numArray = (char[])GetValue(ordinal);
            if (buffer == null) return numArray.Length;
            if (buffer.Length <= bufferIndex) throw new Exception("目标缓冲区起始下标不能大于目标缓冲区范围！");
            var freeLength = Math.Min(numArray.Length - bufferIndex, length);
            if (freeLength <= 0) return 0;
            Array.Copy(numArray, dataIndex, buffer, bufferIndex, length);
            return freeLength;
        }

        public bool IsDBNull(int i)
        {
            var value = GetValue(i);
            return value == null || value is DBNull;
        }
        public bool NextResult()
        {
            //移动到下一个元素
            if (!_enumerable.MoveNext()) return false;
            //行层+1
            Interlocked.Increment(ref _depth);
            //得到数据行
            _currentDataRow = ModelToDataTable<TModel>.ToRowData.Invoke(_enumerable.Current);
            return true;
        }

        public byte GetByte(int i) => (byte)GetValue(i);
        public string GetName(int i) => ModelToDataTable<TModel>.Columns[i].ColumnName;
        public string GetDataTypeName(int i) => ModelToDataTable<TModel>.Columns[i].DataType.Name;
        public Type GetFieldType(int i) => ModelToDataTable<TModel>.Columns[i].DataType;
        public object GetValue(int i) => _currentDataRow[i];
        public bool GetBoolean(int i) => (bool)GetValue(i);
        public char GetChar(int i) => (char)GetValue(i);
        public Guid GetGuid(int i) => (Guid)GetValue(i);
        public short GetInt16(int i) => (short)GetValue(i);
        public int GetInt32(int i) => (int)GetValue(i);
        public long GetInt64(int i) => (long)GetValue(i);
        public float GetFloat(int i) => (float)GetValue(i);
        public double GetDouble(int i) => (double)GetValue(i);
        public string GetString(int i) => (string)GetValue(i);
        public decimal GetDecimal(int i) => (decimal)GetValue(i);
        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
        public IDataReader GetData(int i) => throw new NotSupportedException();
        public int FieldCount => ModelToDataTable<TModel>.Columns.Count;
        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(GetOrdinal(name));
        public void Close() => Dispose();
        public DataTable GetSchemaTable() => ModelToDataTable<TModel>.ToDataTable(_source);
        public bool Read() => NextResult();
        public int Depth => _depth;
        public bool IsClosed => _release;
        public int RecordsAffected => 0;
    }

    /// <summary>
    /// 对象转换成DataTable转换类
    /// </summary>
    /// <typeparam name="TModel">泛型类型</typeparam>
    public static class ModelToDataTable<TModel>
    {
        static ModelToDataTable()
        {
            //如果需要剔除某些列可以修改这段代码
            var propertyList = typeof(TModel).GetProperties().Where(w => w.CanRead).ToArray();
            Columns = new ReadOnlyCollection<DataColumn>(propertyList
                .Select(pr => new DataColumn(pr.Name, GetDataType(pr.PropertyType))).ToArray());

            //生成对象转数据行委托
            ToRowData = BuildToRowDataDelegation(typeof(TModel), propertyList);
        }

        /// <summary>
        /// 构建转换成数据行委托
        /// </summary>
        /// <param name="type">传入类型</param>
        /// <param name="propertyList">转换的属性</param>
        /// <returns>转换数据行委托</returns>
        private static Func<TModel, object[]> BuildToRowDataDelegation(Type type, PropertyInfo[] propertyList)
        {
            var source = Expression.Parameter(type);
            var items = propertyList.Select(property => ConvertBindPropertyToData(source, property));
            var array = Expression.NewArrayInit(typeof(object), items);
            var lambda = Expression.Lambda<Func<TModel, object[]>>(array, source);
            return lambda.Compile();
        }

        /// <summary>
        /// 将属性转换成数据
        /// </summary>
        /// <param name="source">源变量</param>
        /// <param name="property">属性信息</param>
        /// <returns>获取属性数据表达式</returns>
        private static Expression ConvertBindPropertyToData(ParameterExpression source, PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var expression = (Expression)Expression.Property(source, property);
            if (propertyType.IsEnum)
                expression = Expression.Convert(expression, propertyType.GetEnumUnderlyingType());
            return Expression.Convert(expression, typeof(object));
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <returns>数据类型</returns>
        private static Type GetDataType(Type type)
        {
            //枚举默认转换成对应的值类型
            if (type.IsEnum)
                return type.GetEnumUnderlyingType();
            //可空类型
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetDataType(type.GetGenericArguments().First());
            return type;
        }

        /// <summary>
        /// 列集合
        /// </summary>
        public static IReadOnlyList<DataColumn> Columns { get; }

        /// <summary>
        /// 对象转数据行委托
        /// </summary>
        public static Func<TModel, object[]> ToRowData { get; }

        /// <summary>
        /// 集合转换成DataTable
        /// </summary>
        /// <param name="source">集合</param>
        /// <param name="tableName">表名称</param>
        /// <returns>转换完成的DataTable</returns>
        public static DataTable ToDataTable(IEnumerable<TModel> source, string tableName = "TempTable")
        {
            //创建表对象
            var table = new DataTable(tableName);
            //设置列
            foreach (var dataColumn in Columns)
            {
                table.Columns.Add(new DataColumn(dataColumn.ColumnName, dataColumn.DataType));
            }

            //循环转换每一行数据
            foreach (var item in source)
            {
                table.Rows.Add(ToRowData.Invoke(item));
            }

            //返回表对象
            return table;
        }
    }
}
