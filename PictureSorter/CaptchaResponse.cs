using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureSorter
{
     class CaptchaResponse
    {
        public int error_code;
        public string error_msg;
        public object[] request_params;
        public string captcha_sid;
        public string captcha_img;
    }
}
