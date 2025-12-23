using System;
using System.Collections.Generic;
using System.Text;
using WodToolKit.Http;

namespace WodToolKit.src.UmiOCR
{
    public class @base
    {
        public string UmiUrl = "http://api.umiocr.com/v1/ocr";
        public HttpRequestClass http = new HttpRequestClass();
        /// <summary>
        /// 初始化 Umi
        /// <param name="Url">Umi 地址，默认为 "http://127.0.0.1:1224"</param>
        /// </summary>
        public @base(string? Url = "http://127.0.0.1:1224")
        {
            UmiUrl = Url ?? UmiUrl;
        }
    }
}
