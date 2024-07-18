using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBakWeChatDesktop.Model
{
    public class Session
    {
        public int id { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public string wx_id { get; set; }
        public string wx_name { get; set; }
        public string wx_acct_name { get; set; }
        public string wx_key { get; set; }
        public string wx_mobile { get; set; }
        public string wx_email { get; set; }
        public string pid { get; set; }
        public string dir { get; set; }
    }
}
