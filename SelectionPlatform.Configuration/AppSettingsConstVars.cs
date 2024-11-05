using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Configuration
{
    public class AppSettingsConstVars
    {
        #region 全局地址================================================================================
        /// <summary>
        /// 系统后端地址
        /// </summary>
        public static readonly string AppConfigAppUrl = AppSettingsHelper.GetContent("AppConfig", "AppUrl");
        /// <summary>
        /// 系统接口地址
        /// </summary>
        public static readonly string AppConfigAppInterFaceUrl = AppSettingsHelper.GetContent("AppConfig", "AppInterFaceUrl");
        #endregion

        #region 数据库================================================================================
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        public static readonly string DbSqlConnection = AppSettingsHelper.GetContent("ConnectionStrings", "MysqlDB");
      
        #endregion
    }
}
