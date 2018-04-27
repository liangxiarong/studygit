/* 版本与版权说明：-.-Power By Phil
 * Add By Phil 
 * Version 1.0.0.1   Date:2011-XX-XX
 * Version 1.0.0.2   Date:2011-09-01 增加了GetRecordCount查询表中数据条数 
 * Version 1.0.0.3   Date:2011-09-06 增加了GetTopSingleModel查询最顶条数据 
 * Version 1.0.0.4   Date:2011-09-07 增加了IsExistModel判断是否存在当前数据 
 * Version 1.0.0.5   Date:2011-09-09 实体模型时间为DateTime.MinValue时转换成数据库最小时间1900-01-01
 * Version 1.0.0.6   Date:2011-09-26 添加了方法GetModel(string columns,string strWhere)
 * Version 1.0.0.7   Date:2011-11-07 可以更新表的指写字段,Update(string columns,T model)
 * Version 1.0.0.8   Date:2011-11-08 增加了分页获取多条数据时可以指定字段
 */

using System;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace MYSQL
{

    /// <summary>
    /// 容易的数据库访问类
    /// </summary>
    /// <typeparam name="T">数据库表实体类</typeparam>
    public class EasyRecord<T>
    {
        //public  EasyRecord()
        //{
        //   DbHelperMySQL.connectionString= PubConstant.ConnectionString;
        //}
        //public EasyRecord(string ConnectionString)
        //{
        //    DbHelperMySQL.connectionString = ConnectionString;
        //}
        private static EasyRecord<string> _Instance = null;



        /// <summary>
        /// 单例,用于执行
        /// </summary>
        public static EasyRecord<string> Instrance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new EasyRecord<string>();
                } return _Instance;
            }
        }


        #region----------------------增,删,改---------------------------------------------------------
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model">
        /// 实体
        /// </param>
        /// <returns>
        /// SELECT @@IDENTITY
        /// </returns>
        public int Add(T model)
        {
            bool haveNative = false;//判断是否有自动标识
            Type type = typeof(T);
            TableAttribute tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            Dictionary<PropertyInfo, ColumnAttribute> d_ColumnAttribute = new Dictionary<PropertyInfo, ColumnAttribute>();
            foreach (var pInfo in type.GetProperties())
            {
                ColumnAttribute columnAttribute = (ColumnAttribute)Attribute.GetCustomAttribute(pInfo, typeof(ColumnAttribute), true);
                if (columnAttribute == null)
                {
                    continue;
                }
                if (columnAttribute.GenerateType == GenerateType.Native)
                {
                    haveNative = true;
                    continue;
                }
                d_ColumnAttribute.Add(pInfo, columnAttribute);
            }
            StringBuilder strSql = new StringBuilder();
            StringBuilder strItems = new StringBuilder();
            StringBuilder strValues = new StringBuilder();
            MySqlParameter[] parameters = new MySqlParameter[d_ColumnAttribute.Count];
            strSql.Append("INSERT INTO `" + tableAttribute.Name + "`(");
            int i = 0;
            foreach (var d in d_ColumnAttribute)
            {
                strItems.Append("`" + d.Value.Name + "`,");
                strValues.Append("@" + d.Value.Name + ",");
                parameters[i] = this.setParameter(model, d.Key, d.Value);
                i++;
            }
            strSql.Append(strItems.Remove(strItems.Length - 1, 1));
            strSql.Append(") VALUES (");
            strSql.Append(strValues.Remove(strValues.Length - 1, 1));
            strSql.Append(");");
            //如果存在标识，则用SELECT @@IDENTITY
            if (haveNative)
            {
                strSql.Append("SELECT last_insert_id()");
            }

            object obj = DbHelperMySQL.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="lsModel">实体集</param>
        public void AddList(List<T> lsModel)
        {
            foreach (var _Model in lsModel)
            {
                this.Add(_Model);
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="model">
        /// 实体对象
        /// </param>
        /// <returns>
        /// 是否更新成功
        /// </returns>
        public bool Update(T model)
        {
            return this.Update(null, model);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="columns">
        /// 要更新的字段，以逗号进行分开,如果为全部字段更新：null
        /// </param>
        /// <param name="model">
        /// 实体对象
        /// </param>
        /// <returns>
        /// 是否更新成功
        /// </returns>
        public bool Update(string columns, T model)
        {
            Type type = typeof(T);
            TableAttribute tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            Dictionary<PropertyInfo, ColumnAttribute> d_ColumnAttribute = new Dictionary<PropertyInfo, ColumnAttribute>();
            ColumnKeyAttribute columnKeyAttribute = null;
            foreach (var pInfo in type.GetProperties())
            {
                ColumnAttribute columnAttribute = (ColumnAttribute)Attribute.GetCustomAttribute(pInfo, typeof(ColumnAttribute), true);
                if (columnAttribute == null)
                {
                    continue;
                }
                ColumnKeyAttribute tempColumnAttribute = columnAttribute as ColumnKeyAttribute;
                if (tempColumnAttribute != null && columnKeyAttribute == null)
                {
                    columnKeyAttribute = tempColumnAttribute;
                }

                if (tempColumnAttribute != null || columnAttribute.GenerateType == GenerateType.Native)
                {
                    continue;
                }
                //有要更新的字段
                if (columns != null && !new Regex(string.Format(@"(^{0},)|(^{0}$)|(,{0},)|(,{0}$)", columnAttribute.Name.ToLower())).IsMatch(columns.ToLower()))
                {
                    continue;
                }
                d_ColumnAttribute.Add(pInfo, columnAttribute);
            }
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE `" + tableAttribute.Name + "` SET ");
            MySqlParameter[] paras = new MySqlParameter[d_ColumnAttribute.Count + 1];
            int i = 0;
            foreach (var d in d_ColumnAttribute)
            {
                strSql.Append(string.Format("`{0}`=@{0},", d.Value.Name));
                paras[i] = this.setParameter(model, d.Key, d.Value);
                i++;
            }
            strSql = strSql.Remove(strSql.Length - 1, 1);
            strSql.Append(string.Format(" WHERE {0}=@{0} ", columnKeyAttribute.Name));
            var pinfo = type.GetProperty(columnKeyAttribute.Name);
            paras[i] = new MySqlParameter(
                "@" + columnKeyAttribute.Name,
                columnKeyAttribute.Type,
                columnKeyAttribute.Length);
            paras[i].Value = pinfo.GetValue(model, null) ?? "";
            return DbHelperMySQL.ExecuteSql(strSql.ToString(), paras) >= 1;
        }


        /// <summary>
        /// 批量更新数据
        /// </summary>
        /// <param name="lsModels">实体集</param>
        public void UpdateList(List<T> lsModels)
        {
            this.UpdateList(null, lsModels);
        }

        /// <summary>
        /// 批量更新数据
        /// </summary>
        /// <param name="columns">要更新的字段，以逗号进行分开,如果为全部字段更新：null</param>
        /// <param name="lsModels">实体集</param>
        public void UpdateList(string columns, List<T> lsModels)
        {
            foreach (var model in lsModels)
            {
                this.Update(columns, model);
            }
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(int id)
        {
            return this.DeleteByID(id);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool DeleteByID(object id)
        {
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            PropertyInfo pinfo = type.GetProperty(table.Name);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM `" + table.Name + "` ");
            MySqlParameter[] paras = new MySqlParameter[1];
            foreach (var pInfo in type.GetProperties())
            {
                ColumnKeyAttribute columnKey = (ColumnKeyAttribute)Attribute.GetCustomAttribute(pInfo, typeof(ColumnKeyAttribute), false);
                if (columnKey != null)
                {
                    strSql.Append(string.Format(" WHERE {0}=@{0} ", columnKey.Name));
                    paras[0] = new MySqlParameter("@" + columnKey.Name, columnKey.Type, columnKey.Length);
                    paras[0].Value = id;
                    break;
                }
            }
            return DbHelperMySQL.ExecuteSql(strSql.ToString(), paras) == 1;
        }


        /// <summary>
        /// 删除条件包含的数据
        /// </summary>
        public bool Delete(string strWhere)
        {
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            PropertyInfo pinfo = type.GetProperty(table.Name);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM `" + table.Name + "` WHERE " + strWhere);
            return DbHelperMySQL.ExecuteSql(strSql.ToString()) >= 1;
        }
        /// <summary>
        /// 删除全部
        /// </summary>
        public void DeleteAll()
        {
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            PropertyInfo pinfo = type.GetProperty(table.Name);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM `" + table.Name + "` ");
            DbHelperMySQL.ExecuteSql(strSql.ToString());
        }

        #endregion----------------------增,删,改---------------------------------------------------------

        #region -------------------------------------单例--------------------------------------
        /// <summary>
        /// 得到一个对象实体 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetModel(int id)
        {
            return this.GetSingleModel(id);
        }

        /// <summary>
        /// 得到一个对象实体 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetModel(long id)
        {
            return this.GetSingleModel(id);
        }

        /// <summary>
        /// 得到一个对象实体   
        /// </summary>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public T GetModel(string strWhere)
        {
            return this.GetModel("*", strWhere);
        }

        public T GetModelOrderBy(string strWhere, string OrderBy)
        {
            return this.GetModel("*", strWhere, OrderBy);
        }

        /// <summary>
        /// 得到一个对象实体   
        /// </summary>
        /// <param name="columns">
        /// 要获得的字段
        /// </param>
        /// <param name="strWhere">
        /// 条件
        /// </param>
        /// <returns>
        /// 实体对象
        /// </returns>
        public T GetModel(string columns, string strWhere, string OrderBy = "")
        {
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT     " + columns + " FROM  `" + table.Name + "`  ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            if (!string.IsNullOrEmpty(OrderBy))
            {
                strSql.AppendFormat(" Order by {0}", OrderBy);
            }
            DataSet ds = DbHelperMySQL.Query(strSql.ToString());
            T model = (T)Activator.CreateInstance(type);
            if (ds.Tables[0].Rows.Count > 0)
            {
                this.dsToModel(ds.Tables[0].Rows[0], model);
            }
            return model;
        }

        /// <summary>
        /// /得到一个对象实体 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetSingleModel(object id)
        {
            return GetSingleModel(id, "*");
        }

        /// <summary>
        /// /得到一个对象实体 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="columns">列名</param>
        /// <returns></returns>
        public T GetSingleModel(object id, string columns)
        {
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT     " + columns + " FROM  `" + table.Name + "`   ");
            MySqlParameter[] paras = new MySqlParameter[1];
            foreach (var pInfo in type.GetProperties())
            {
                ColumnKeyAttribute columnKey = (ColumnKeyAttribute)Attribute.GetCustomAttribute(pInfo, typeof(ColumnKeyAttribute), false);
                if (columnKey != null)
                {
                    strSql.Append(string.Format(" WHERE {0}=@{0} ", columnKey.Name));
                    paras[0] = new MySqlParameter("@" + columnKey.Name, columnKey.Type, columnKey.Length);
                    paras[0].Value = id;
                }
            }
            DataSet ds = DbHelperMySQL.Query(strSql.ToString(), paras);
            T model = (T)Activator.CreateInstance(type);
            if (ds.Tables[0].Rows.Count > 0)
            {
                this.dsToModel(ds.Tables[0].Rows[0], model);
            }
            return model;
        }

        /// <summary>
        /// 查询最顶的一条数据
        /// </summary>
        /// <returns>数据对象</returns>
        public T GetTopSingleModel()
        {
            return this.GetTopSingleModel("", "*");
        }
        /// <summary>
        /// 查询最顶的一条数据
        /// </summary>
        /// <param name="orderby">排序</param>
        /// <returns>数据对象</returns>
        public T GetTopSingleModel(string orderby)
        {
            return this.GetTopSingleModel(orderby, "*");
        }
        /// <summary>
        /// 查询最顶的一条数据
        /// </summary>
        /// <param name="orderby">排序</param>
        /// <param name="columns">查询列</param>
        /// <returns>数据对象</returns>
        public T GetTopSingleModel(string orderby, string columns)
        {
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT     " + columns + " FROM  `" + table.Name + "`   order by " + orderby);
            DataSet ds = DbHelperMySQL.Query(strSql.ToString());
            T model = (T)Activator.CreateInstance(type);
            if (ds.Tables[0].Rows.Count > 0)
            {
                this.dsToModel(ds.Tables[0].Rows[0], model);
            }
            return model;
        }
        #endregion -------------------------------------单例--------------------------------------

        #region -------------------------多条数据------------------------------------------------
        /// <summary>
        /// 获得表中所有的数据
        /// </summary>
        public List<T> GetModelList()
        {
            return GetModelList(string.Empty, "*");
        }

        /// <summary>
        /// 获得条件数据
        /// </summary>
        public List<T> GetModelList(string strWhere)
        {
            return GetModelList(strWhere, "*");
        }

        /// <summary>
        /// 获得条件数据
        /// </summary>
        /// <param name="strWhere">
        /// 查询条件
        /// </param>
        /// <param name="columns">
        /// 字段名列
        /// </param>
        /// <returns>
        /// 实体集
        /// </returns>
        public List<T> GetModelList(string strWhere, string columns)
        {
            return this.GetModelList(0, strWhere, columns);
        }

        /// <summary>
        /// 获得条件数据，并指定条数
        /// </summary>
        /// <param name="topCount">
        /// 多少条
        /// </param>
        /// <param name="strWhere">
        /// 查询条件
        /// </param>
        /// <param name="columns">
        /// 字段名列
        /// </param>
        /// <returns>
        /// 实体集
        /// </returns>
        public List<T> GetModelList(int topCount, string strWhere, string columns)
        {
            Type type = typeof(T);
            List<T> lt = new List<T>();
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();

            strSql.Append("SELECT  " + columns + " FROM  `" + table.Name + "` ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append("WHERE 1=1 and " + strWhere);
            }
            if (topCount > 0)
            {
                strSql.AppendFormat(" limit {} ", topCount);
            }
            DataSet ds = DbHelperMySQL.Query(strSql.ToString());
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    T model = (T)Activator.CreateInstance(type);
                    this.dsToModel(dr, model);
                    lt.Add(model);
                }
            }
            return lt;
        }




        /// <summary>
        /// 获得条件数据，并指定条数
        /// </summary>
        /// <param name="topCount">
        /// 多少条
        /// </param>
        /// <param name="strWhere">
        /// 查询条件
        /// </param>
        /// <param name="columns">
        /// 字段名列
        /// </param>
        /// <param name="orderby">
        /// 排序
        /// </param>
        /// <returns>
        /// 实体集
        /// </returns>
        public List<T> GetModelList(int topCount, string strWhere, string columns, string orderby)
        {
            Type type = typeof(T);
            List<T> lt = new List<T>();
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();

            if (string.IsNullOrEmpty(columns))
            {
                columns = "*";
            }

            strSql.Append("SELECT  "  + columns + " FROM  `" + table.Name + "` ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append(" WHERE 1=1 and " + strWhere);
            }
            if (!orderby.Trim().Equals(""))
            {
                strSql.Append(" Order By " + orderby);
            }
            if (topCount>0)
            {
                strSql.AppendFormat(" limit {0} ", topCount);
            }
            DataSet ds = DbHelperMySQL.Query(strSql.ToString());
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    T model = (T)Activator.CreateInstance(type);
                    this.dsToModel(dr, model);
                    lt.Add(model);
                }
            }
            return lt;
        }

        /// <summary>
        /// 分页实体数据集
        /// </summary>
        /// <param name="columns">字段,*号时为表中全部字段</param>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrder">排序</param>
        /// <param name="page">当前页</param>
        /// <param name="pageSize">每次条数</param>
        /// <param name="recordCount">总共条数</param>
        /// <returns>实体数据集</returns>
        public List<T> GetPagerModelList(string columns, string strWhere, string strOrder, int page, int pageSize, ref int recordCount)
        {
            Type type = typeof(T);
            List<T> lt = new List<T>();
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();

            strSql.Append("SELECT COUNT(1) FROM `" + table.Name + "` ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            object obj = this.GetSingle(strSql.ToString());

            recordCount = ((obj == null) ? 0 : int.Parse(obj.ToString()));

            strSql = new StringBuilder();

            strSql.Append("SELECT " + columns + "  FROM `" + table.Name + "` ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            strSql.Append(string.Format(" order by  {0} limit {1},{2}", strOrder,(page - 1) * pageSize + 1,  pageSize));
            DataSet ds = DbHelperMySQL.Query(strSql.ToString());
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    T model = (T)Activator.CreateInstance(type);
                    this.dsToModel(dr, model);
                    lt.Add(model);
                }
            }
            return lt;
        }

        /// <summary>
        /// 分页实体数据集
        /// </summary>
        /// <param name="strOrder">排序</param>
        /// <param name="page">当前页</param>
        /// <param name="pageSize">每次条数</param>
        /// <param name="recordCount">总共条数</param>
        /// <returns>实体数据集</returns>
        public List<T> GetPagerModelList(string strOrder, int page, int pageSize, ref int recordCount)
        {
            return this.GetPagerModelList("", strOrder, page, pageSize, ref recordCount);
        }

        /// <summary>
        /// 分页实体数据集
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrder">排序</param>
        /// <param name="page">当前页</param>
        /// <param name="pageSize">每次条数</param>
        /// <param name="recordCount">总共条数</param>
        /// <returns>实体数据集</returns>
        public List<T> GetPagerModelList(string strWhere, string strOrder, int page, int pageSize, ref int recordCount)
        {
            return this.GetPagerModelList("*", strWhere, strOrder, page, pageSize, ref recordCount);
        }


        /// <summary>
        /// 分页数据集
        /// </summary>
        /// <param name="columns">字段</param>
        /// <param name="strWhere">条件</param>
        /// <returns>数据集</returns>
        public DataSet GetList(string columns, string strWhere)
        {
            Type type = typeof(T);
            List<T> lt = new List<T>();
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();
            strSql.Append(string.Format("SELECT  {0} FROM  `" + table.Name + "` "
                , columns));
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append("WHERE " + strWhere);
            }
            return DbHelperMySQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 分页数据集
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <returns>数据集</returns>
        public DataSet GetList(string strWhere)
        {
            return this.GetList("*", strWhere);
        }
        /// <summary>
        /// 获得分页数据集
        /// </summary>
        /// <param name="columns">字段,*号时为表中全部字段</param>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrder">排序</param>
        /// <param name="page">当前页</param>
        /// <param name="pageSize">每次条数</param>
        /// <param name="recordCount">总共条数</param>
        /// <returns>DataSet 数据集</returns>
        public DataSet GetPagerList(string columns, string strWhere, string strOrder, int page, int pageSize, ref int recordCount)
        {
            Type type = typeof(T);
            List<T> lt = new List<T>();
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT COUNT(1) FROM `" + table.Name + "` ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            object obj = this.GetSingle(strSql.ToString());

            recordCount = ((obj == null) ? 0 : int.Parse(obj.ToString()));
            strSql = new StringBuilder();
        
            strSql.Append("SELECT  " + columns + "  FROM `" + table.Name + "` ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            strSql.Append(string.Format(" order by  {0} limit {1},{2}", strOrder, (page - 1) * pageSize + 1, pageSize));
            return DbHelperMySQL.Query(strSql.ToString());
        }
        /// <summary>
        /// 获得分页数据集
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrder">排序</param>
        /// <param name="page">当前页</param>
        /// <param name="pageSize">每次条数</param>
        /// <param name="recordCount">总共条数</param>
        /// <returns>DataSet 数据集</returns>
        public DataSet GetPagerList(string strWhere, string strOrder, int page, int pageSize, ref int recordCount)
        {

            return this.GetPagerList(" * ", strWhere, strOrder, page, pageSize, ref recordCount);
        }

        /// <summary>
        /// 获得分页数据集
        /// </summary>
        /// <param name="strOrder">排序</param>
        /// <param name="page">当前页</param>
        /// <param name="pageSize">每次条数</param>
        /// <param name="recordCount">总共条数</param>
        /// <returns>DataSet 数据集</returns>
        public DataSet GetPagerList(string strOrder, int page, int pageSize, ref int recordCount)
        {
            return this.GetPagerList("", strOrder, page, pageSize, ref recordCount);
        }

        #endregion  -------------------------多条数据------------------------------------------------

        #region 逻辑操作
        /// <summary>
        /// 表中数据是否存在
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <returns>存在状态</returns>
        public bool IsExistModel(string strWhere)
        {
            bool _blExist = false;
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT  COUNT(1) FROM  `" + table.Name + "` ");
            if (!strWhere.Equals(""))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            object obj = DbHelperMySQL.GetSingle(strSql.ToString());
            if (int.Parse(obj.ToString()) > 0)
            {
                _blExist = true;
            }
            return _blExist;
        }
        #endregion

        #region -----------------------------公有方法-----------------------------------------------
        /// <summary>
        /// 根据条件获取表中的数据条数
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <returns>条数</returns>
        public int GetRecordCount(string strWhere)
        {
            var table = (TableAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableAttribute), false);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT  COUNT(1) FROM  `" + table.Name + "` ");
            if (!strWhere.Trim().Equals(""))
            {
                strSql.Append("WHERE " + strWhere);
            }
            var _object = DbHelperMySQL.GetSingle(strSql.ToString());
            return int.Parse(_object.ToString());
        }
        /// <summary>
        /// 获取表中的所有数据条数
        /// </summary>
        /// <returns>数据条数</returns>
        public int GetRecordCount()
        {
            return this.GetRecordCount(string.Empty);
        }
        public bool ExecuteMoreSql(string strSql)
        {
            return DbHelperMySQL.TransactionWithSql(strSql);
        }
        /// <summary>
        /// 执行SqlReader
        /// </summary>
        /// <param name="strSql">查询语句</param>
        /// <returns>SqlReader</returns>
        public MySqlDataReader ExecuteReaderSql(string strSql)
        {
            return DbHelperMySQL.ExecuteReader(strSql);
        }

        /// <summary>
        /// 执行查询语句返回DataSet
        /// </summary>
        /// <param name="strSql">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDSSql(string strSql)
        {
            return DbHelperMySQL.Query(strSql);
        }

        public DataSet ExecuteDSProc(string ProName,IDataParameter[] parameters)
        {
            return DbHelperMySQL.RunProcedureForDs(ProName,parameters);
        }


        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="strSql">全部查询语句</param>
        /// <param name="strOrder">排序</param>
        /// <param name="page">当前页数</param>
        /// <param name="pageSize">每次条数</param>
        /// <param name="recordCount">所有数据条数</param>
        /// <returns></returns>
        public DataSet ExecutePagerDSSql(
            string strSql,
            string strOrder,
            int page,
            int pageSize,
            ref int recordCount)
        {
            StringBuilder _sbSql = new StringBuilder();
            _sbSql.Append("SELECT COUNT(1) FROM ( ");
            _sbSql.Append(strSql);
            _sbSql.Append(" ) AS PagerTable");
            object obj = DbHelperMySQL.GetSingle(_sbSql.ToString());
            recordCount = ((obj == null) ? 0 : int.Parse(obj.ToString()));

            _sbSql = new StringBuilder();
            _sbSql.Append(
                string.Format(@"
SELECT * FROM (
SELECT
ROW_NUMBER() OVER (ORDER BY {0}) AS [PageIndex] ,* FROM ( "
                , strOrder
                ));
            _sbSql.Append(strSql);
            _sbSql.Append(" ) AS TempTable ) AS  PagerTable");
            _sbSql.Append(
                string.Format(" WHERE PagerTable.[PageIndex] BETWEEN {0} AND {1}"
                , (page - 1) * pageSize + 1
                , page * pageSize
                ));

            return DbHelperMySQL.Query(_sbSql.ToString());
        }

        public int ExecuteSql(string strSql)
        {
            return DbHelperMySQL.ExecuteSql(strSql);
        }
        public object GetSingle(string strSql)
        {
            return DbHelperMySQL.GetSingle(strSql);
        }
        /// <summary>
        /// 是否存在,以Count方式查询
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public bool IsExist(string strSql)
        {
            object obj = DbHelperMySQL.GetSingle(strSql);
            if (obj==null)
            {
                return false;
            }
            return (int.Parse(obj.ToString()) == 0) ? false : true;
        }


        #endregion -----------------------------公有方法-----------------------------------------------

        #region --------------------------------私有方法------------------------------------------------
        public List<T> dsToList(DataSet ds)
        {
            Type type = typeof(T);
            List<T> lst = new List<T>();
            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    T model = (T)Activator.CreateInstance(type);
                    this.dsToModel(dr, model);
                    lst.Add(model);
                }
            }
            return lst;
        }
        //赋值
        public void dsToModel(DataRow dr, T model)
        {
            Type type = typeof(T);
            foreach (var pInfo in type.GetProperties())
            {
                //ColumnAttribute column = (ColumnAttribute)Attribute.GetCustomAttribute(pInfo, typeof(ColumnAttribute), true);
                var cName = pInfo.Name;//属性名
                if (dr.Table.Columns.Contains(cName))
                {
                    if (dr[cName] != null)//&& dr[column.Name ] != DBNull.Value
                    {
                        if (dr[cName] != DBNull.Value)
                        {
                            pInfo.SetValue(model, dr[cName], null);
                        }
                        else
                        {
                            //若为null，怎根据类型返回
                            //字符串 返回空字符 bool 返回false int32 返回 0
                            var typeName = pInfo.PropertyType.Name

;
                            if (typeName == "Boolean")
                            {
                                pInfo.SetValue(model, false, null);
                            }
                            else if (typeName == "Int32")
                            {
                                pInfo.SetValue(model, 0, null);
                            }
                            else if (typeName == "String")
                            {
                                //返回空字符
                                pInfo.SetValue(model, "", null);
                            }

                        }

                    }
                }
                else
                {
                    //返回的列不包含公共属性
                    if (pInfo.PropertyType == Type.GetType("System.String"))
                    {
                        pInfo.SetValue(model, "", null);
                    }
                }
            }
        }





        /// <summary>
        /// 生成MySqlParameter
        /// </summary>
        /// <param name="Model">实体模型</param>
        /// <param name="pInfo">实体模型的一个属性</param>
        /// <param name="columnAttribute">属性关系模型</param>
        /// <returns>MySqlParameter</returns>
        private MySqlParameter setParameter(
            T Model,
            PropertyInfo pInfo,
            ColumnAttribute columnAttribute)
        {
            MySqlParameter MySqlParameter
                = new MySqlParameter(
                "@" + columnAttribute.Name,
                columnAttribute.Type,
                columnAttribute.Length);
            var _Value = pInfo.GetValue(Model, null) ?? "";
            if ((columnAttribute.Type == MySqlDbType.DateTime
                || columnAttribute.Type == MySqlDbType.Date)
                && DateTime.Parse(_Value.ToString()) == DateTime.MinValue)
            {
                //_Value = "1900-01-01";
                _Value =new DateTime(1900,1,1,0,0,0);

            }
            MySqlParameter.Value = _Value;
            return MySqlParameter;
        }
        #endregion --------------------------------私有方法------------------------------------------------
        /// <summary>
        /// 修改字段值
        /// </summary>
        /// <param name="culume">标识字段</param>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public bool ChangeColumn(string culume, long id)
        {
            Type type = typeof(T);
            TableAttribute table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            PropertyInfo pinfo = type.GetProperty(table.Name);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update `" + table.Name + "` set " + culume + "  WHERE  id=" + id);
            return DbHelperMySQL.ExecuteSql(strSql.ToString()) >= 1;
        }
    }
}
