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
    [Table("student")]
    public partial class student
    {
        #region Fields
        private uint _id;
        private string _name;
        #endregion
        
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        [ColumnKey(GenerateType.Native,"id",MySqlDbType.Int32,0)]
        public uint id
        {
            get{return _id;}
            set{_id=value ;}
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Column("name",MySqlDbType.VarChar,300)]
        public string name
        {
            get{return _name;}
            set{_name=value ;}
        }
        
        #endregion        
    }
}



