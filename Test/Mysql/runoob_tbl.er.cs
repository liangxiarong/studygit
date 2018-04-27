using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
namespace MySQL.Model 
{
    /// <summary>
    ///
    /// Auto Generate: Phil 2018-04-18 14:16:55 
    /// </summary>
    [Table("runoob_tbl")]
    public partial class runoob_tbl
    {
        #region Fields
        private uint _runoob_id;
        private string _runoob_title;
        private string _runoob_author;
        private DateTime _submission_date;
        #endregion
        
        #region Properties

        /// <summary>
        /// Id组件
        /// </summary>
        [ColumnKey(GenerateType.Native,"runoob_id",MySqlDbType.Int32,0)]
        public uint runoob_id
        {
            get{return _runoob_id;}
            set{_runoob_id=value ;}
        }
        
        /// <summary>
        /// 标题
        /// </summary>
        [Column("runoob_title",MySqlDbType.VarChar,300)]
        public string runoob_title
        {
            get{return _runoob_title;}
            set{_runoob_title=value ;}
        }
        
        /// <summary>
        /// 作者
        /// </summary>
        [Column("runoob_author",MySqlDbType.VarChar,120)]
        public string runoob_author
        {
            get{return _runoob_author;}
            set{_runoob_author=value ;}
        }
        
        /// <summary>
        /// 日期
        /// </summary>
        [Column("submission_date",MySqlDbType.DateTime,0)]
        public DateTime submission_date
        {
            get{return _submission_date;}
            set{_submission_date=value ;}
        }
        
        #endregion        
    }
}



