using AllPay.Payment.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AllpayWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #region 線上金流
        
        public ActionResult Pay()
        {
            string result = ProcessPayment();
            return Content(result);
        }

        public string ProcessPayment()
        {
            List<string> enErrors = new List<string>();
            string szHtml = String.Empty;
            try
            {
                using (AllInOne oPayment = new AllInOne())
                {
                    /* 服務參數 */
                    oPayment.ServiceMethod = HttpMethod.HttpPOST;
                    oPayment.ServiceURL = "https://payment-stage.allpay.com.tw/Cashier/AioCheckOut/V2";
                    oPayment.HashKey = "5294y06JbISpM5x9";
                    oPayment.HashIV = "v77hoKGq4kWxNNIS";
                    oPayment.MerchantID = "2000132";

                    /* 基本參數 */
                    string hostname = this.Request.Url.Authority;
                    oPayment.Send.ReturnURL = $"http://{hostname}/AllPayPayment/CallBack";
                    oPayment.Send.MerchantTradeNo = DateTime.Now.ToString("yyyyMMddHHmmss");
                    oPayment.Send.MerchantTradeDate = DateTime.Now;
                    oPayment.Send.TotalAmount = 1;
                    oPayment.Send.TradeDesc = "測試金流的描述 ABC";
                    oPayment.Send.Items.Add(new Item()
                    {
                        Name = "iPhone6 Plus"
                        , Price = 99
                        , Currency = "元"
                        , Quantity = 1
                        //, Unit = "組"
                        //, TaxType = TaxationType.Taxable
                    }
                    );
                    /* 產生訂單 */
                    enErrors.AddRange(oPayment.CheckOut());         
                    /* 產生產生訂單 Html Code 的方法 */
                    enErrors.AddRange(oPayment.CheckOutString(ref szHtml));
                }
            }
            catch (Exception ex)
            {
                // 例外錯誤處理。 
                enErrors.Add(ex.Message);
            }
            finally
            {
                // 顯示錯誤訊息。 
                if (enErrors.Count() > 0)
                {
                    szHtml = String.Join("\\r\\n", enErrors);
                }
            }
            return szHtml;
        }

        #endregion

        #region 電子地圖串接
        /// <summary>
        /// 開啟超商電子地圖
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenMap()
        {
            string result = GeneratorHtmlforOpenMap();
            return Content(result);
        }

        /// <summary>
        /// 產生電子地圖串接的前端 HTML
        /// </summary>
        /// <returns></returns>
        private string GeneratorHtmlforOpenMap()
        {
            //讀取範本修改參數
            string hostname = this.Request.Url.Authority;
            string fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/OpenMap.html"));

            return fileContents.Replace("http://localhost/Home/LogisticsReturn"
                , $"http://{hostname}/Home/LogisticsReturn");
        }

        /// <summary>
        /// 綠界回傳的資訊
        /// </summary>
        /// <returns></returns>
        public ActionResult LogisticsReturn()
        {
            string content = "";
            foreach (string key in this.Request.Form.AllKeys)
            {
                string value = this.Request.Form[key];
                content += $"{key}={value}<br/>";
            }
            return Content(content);
        }
        #endregion
    }
}