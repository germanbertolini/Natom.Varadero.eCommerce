using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace natom.varadero.ecomm.Controllers
{
    public class BaseController : Controller
    {
        public string SesionToken
        {
            get
            {
                HttpCookie cookie = Request.Cookies["t"];
                if (cookie == null)
                {
                    return null;
                }
                else
                {
                    return cookie.Value;
                }
            }
        }

        public string SesionTokenDashboard
        {
            get
            {
                HttpCookie cookie = Request.Cookies["d"];
                if (cookie == null)
                {
                    return null;
                }
                else
                {
                    return cookie.Value;
                }
            }
        }

        public int? PedidoId
        {
            get
            {
                HttpCookie cookie = Request.Cookies["c"];
                if (cookie == null)
                {
                    return null;
                }
                else
                {
                    return Convert.ToInt32(cookie.Value);
                }
            }
        }

        public string UserAgent
        {
            get
            {
                return Request.UserAgent;
            }
        }

        public string UserIP
        {
            get
            {
                return Request.UserHostAddress;
            }
        }
    }
}