using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 数据生成类型，Identity时为Native
    /// </summary>
    public enum GenerateType
    {
        Generator = 0,
        Native = 1//自动标识
    }

    /// <summary>
    /// 字段
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute() { }

        public ColumnAttribute(string name, MySqlDbType type, int length)
        {
            this.Name = name;
            this.Type = type;
            this.Length = length;
        }

        public ColumnAttribute(GenerateType generateType, string name, MySqlDbType type, int length)
        {
            this._GenerateType = generateType;
            this.Name = name;
            this.Type = type;
            this.Length = length;
        }

        /// <summary>
        /// 字段名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public MySqlDbType Type { get; set; }
        /// <summary>
        /// 字段长度
        /// </summary>
        public int Length { get; set; }

        private GenerateType _GenerateType = GenerateType.Generator;
        /// <summary>
        /// 是否为标识
        /// </summary>
        public GenerateType GenerateType
        {
            get { return _GenerateType; }
            set { _GenerateType = value; }
        }
    }

    /// <summary>
    /// 主键
    /// </summary>
    public class ColumnKeyAttribute : ColumnAttribute
    {
        public ColumnKeyAttribute(string name, MySqlDbType type, int length)
            : base(name, type, length)
        {
            this.GenerateType = GenerateType.Native;
        }

        public ColumnKeyAttribute(GenerateType generateType, string name, MySqlDbType type, int length)
            : base(generateType, name, type, length)
        {
        }
    }

    /// <summary>
    /// 记录表名
    /// </summary>
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; }
    }
}
