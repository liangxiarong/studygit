using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MYSQL;
using System.Data;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            int re = 0;
            var dal = new EasyRecord<string>();
            var ds = dal.ExecuteDSSql("select * from users");
            if (ds != null)
            {
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = ds.Tables[0].Rows[i];
                        //Console.WriteLine(item.);
                        var value = string.Empty;
                        foreach (DataColumn c_item in ds.Tables[0].Columns)
                        {
                            value += "   " + dr[c_item];
                        }
                        Console.WriteLine(string.Format("第{0}行 {1}", i + 1, value));
                    }

                }
            }
            //for (int i = 0; i < 1000000; i++)
            //{
            //    Console.WriteLine(dal.Add(new MySQL.Model.tb1
            //    {
            //        Name = "连接字符串",
            //        Blog = "连接字符串连接字符串"
            //    }));
            //}

            //var model = new EasyRecord<MySQL.Model.runoob_tbl>().GetModelList(20, "", "", " runoob_id asc  ");

            //{
            //    new EasyRecord<MySQL.Model.runoob_tbl>().Add(new MySQL.Model.runoob_tbl()
            //    {
            //        runoob_author =$"runoob_author{i}",
            //         runoob_title=$"title{i}",
            //          submission_date=DateTime.UtcNow

            //});
            //}
            /*
              var sql = @"select top 29 * from (SELECT  a.[Tsn],v.GpsTime,v.Geo FROM [QILOO_Language].[dbo].[tTerminalDetail]  a inner join [QILOO_Language].[dbo].[tTerminal] b on a.Tsn=b.tsn  left join tProduct t on b.ProductID_Type=t.id left join  tTerminalView v on a.Tsn=v.Tsn  left join [综合信息查询] tv on tv.卡号=a.WatchPhone where Geo<>'' and Geo is not null  and Geo not like '%和平路%' and   Geo not like '%松白路%'  and Geo not like '%淑女路%' and Geo not like '%宝华路%' and ProductID_Type in (1,7,8) union SELECT a.[Tsn],v.GpsTime,v.Geo FROM [QILOO_Language].[dbo].[tTerminalDetail]  a inner join [QILOO_Language].[dbo].[tTerminal] b on a.Tsn=b.tsn  left join tProduct t on b.ProductID_Type=t.id left join  tTerminalView v on a.Tsn=v.Tsn  left join TphonePackage20170616 tp on tp.Tel=a.WatchPhone where Geo<>'' and Geo is not null    and Geo not like '%和平路%' and   Geo not like '%松白路%'  and Geo not like '%淑女路%'  and Geo not like '%宝华路%'  and ProductID_Type in (1,7,8)) m where  m.Tsn not in ( select distinct Tsn from StepData) order by GpsTime desc";
              var ds = new GPS.Dal.EasyRecord<string>().ExecuteDSSql(sql);
              List<string> list = new List<string>();
              for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
              {
                  var dr = ds.Tables[0].Rows[i];
                  var tsn= dr["Tsn"].ToString();
                  var GpsTime = DateTime.Parse(dr["GpsTime"].ToString());
                  var terminal= new GPS.Dal.EasyRecord<GPS.Model.tTerminal>().GetModel($"Tsn='{tsn}'");
                  var sqlStr = $"select * from z{GpsTime.ToString("yyMM")} where mid='{terminal.ID}' AND datediff(hh,gpsTime,'{GpsTime}')<=2";
                  var locList= new GPS.Dal.EasyRecord<string>().ExecuteDSSql(sqlStr);
                  if (locList.Tables[0].Rows.Count>=2)
                  {
                      var lat1 = double.Parse(locList.Tables[0].Rows[0]["la"].ToString());
                      var lo1 = double.Parse(locList.Tables[0].Rows[0]["lo"].ToString());
                      for (int k = 0; k < locList.Tables[0].Rows.Count; k++)
                      {


                          var lat = double.Parse(locList.Tables[0].Rows[k]["la"].ToString());
                          var lo = double.Parse(locList.Tables[0].Rows[k]["lo"].ToString());
                          double r = EarthUtils.distVincenty(lat1, lo1, lat, lo);
                          if (r > 50)
                          {
                              list.Add(tsn);
                              break;
                          }
                      }

                  }



              }
              AppLog.log.AddLogForXF("123",string.Join(",",list));
              */
            Console.Read();
        }
    }
}
