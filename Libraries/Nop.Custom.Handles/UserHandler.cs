using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Newtonsoft.Json;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Data;

namespace Nop.Custom.Handles
{
    public class UserHandler : IHttpHandler
    {
        /// <summary>
        /// 您将需要在网站的 Web.config 文件中配置此处理程序 
        /// 并向 IIS 注册它，然后才能使用它。有关详细信息，
        /// 请参见下面的链接: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // 如果无法为其他请求重用托管处理程序，则返回 false。
            // 如果按请求保留某些状态信息，则通常这将为 false。
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string result = GetCustomerJson(1);
            context.Response.Write(result);
        }

        public string GetCustomerJson(int customerId)
        {
            var settings = new DataSettings()
            {
                DataProvider = "sqlserver",
                DataConnectionString = ConfigurationManager.ConnectionStrings["serviceDB"].ToString()
            };

            var dataProviderSettings = settings;

            NopObjectContext dbContext = new NopObjectContext(settings.DataConnectionString);
            EfRepository<Customer> repository = new Data.EfRepository<Customer>(dbContext);

            Customer customer = repository.GetById(customerId);


            IEnumerable<Customer> customers = repository.Table;

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string result = JsonConvert.SerializeObject(new { Customer = customer }, Formatting.Indented, jsonSettings);

            return result;
        }

        #endregion
    }
}
