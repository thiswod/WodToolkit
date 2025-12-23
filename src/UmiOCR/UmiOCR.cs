using Jint;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WodToolKit.Http;

namespace WodToolKit.src.UmiOCR
{
    public class UmiOCR
    {
        private string UmiOCRUrl = "http://api.umiocr.com/v1/ocr";
        private HttpRequestClass http = new HttpRequestClass();
        /// <summary>
        /// 初始化 UmiOCR
        /// <param name="Url">UmiOCR 地址，默认为 "http://127.0.0.1:1224"</param>
        /// </summary>
        public UmiOCR(string? Url = "http://127.0.0.1:1224")
        {
            UmiOCRUrl = Url ?? UmiOCRUrl;
        }
        /// <summary>
        /// 执行 OCR 识别
        /// </summary>
        /// <param name="file">图片文件路径</param>
        /// <param name="language">识别语言，默认为 "ch"（中文），可选值：ch（中文）、en（英文）、ch+en（中英混合）等</param>
        /// <param name="format">输出格式，默认为 "text"（纯文本），可选值：text、json、jsonl</param>
        /// <param name="parser">后处理解析器，默认为 "none"（无），可选值：none、merge_line、merge_line_v2 等</param>
        /// <param name="angle">是否自动检测图片角度，默认为 false（不检测）</param>
        /// <param name="ignoreArea">忽略区域数组，每一项为[[左上角x,y],[右下角x,y]]，默认为空数组</param>
        /// <returns>OCR 识别结果</returns>
        public string Ocr(string file, string language = "简体中文", string format = "text", string parser = "none", bool angle = false, List<int[][]> ignoreArea = null)
        {
            http.Open($"{UmiOCRUrl}/api/ocr",HttpMethod.Post);
            string Base64Img = Common.Common.Base64Encode(file);

            var options = new Dictionary<string, object>
            {
                { "tbpu.parser", parser },
                { "data.format", format }
            };
            
            // 添加语言参数
            if (!string.IsNullOrEmpty(language))
            {
                options["ocr.language"] = language;
            }

            // 添加角度参数
            options["ocr.angle"] = angle;

            // 添加忽略区域参数
            if (ignoreArea != null && ignoreArea.Count > 0)
            {
                options["ocr.ignoreArea"] = ignoreArea;
            }
            http.Send(new
            {
                base64 = Base64Img,
                options = options
            });
            try
            {
                return http.GetResponse().Body;
            }
            catch (Exception ex)
            {
                throw new Exception("OCR 识别失败: " + ex.Message);
            }
        }
    }
}
