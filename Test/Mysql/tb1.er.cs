using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
namespace MySQL.Model 
{
    /// <summary>
    ///
    /// Auto Generate: Phil 2018-04-18 16:16:22 
    /// </summary>
    [Table("tb1")]
    public partial class tb1
    {
        #region Fields
        private int _Id;
        private string _Name;
        private string _Blog;
        #endregion
        
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        [ColumnKey(GenerateType.Native,"Id",MySqlDbType.Int32,0)]
        public int Id
        {
            get{return _Id;}
            set{_Id=value ;}
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Column("Name",MySqlDbType.VarChar,60)]
        public string Name
        {
            get{return _Name;}
            set{_Name=value ;}
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Column("Blog",MySqlDbType.VarChar,1500)]
        public string Blog
        {
            get{return _Blog;}
            set{_Blog=value ;}
        }
        
        #endregion        
    }
}



