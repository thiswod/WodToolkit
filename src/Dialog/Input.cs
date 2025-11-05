using System;

namespace WodToolkit.src.Dialog
{
    /// <summary>
    /// 输入对话框结果
    /// </summary>
    public class InputDialogResult
    {
        /// <summary>
        /// 用户是否点击了确定
        /// </summary>
        public bool IsOk { get; set; }
        
        /// <summary>
        /// 用户输入的文本
        /// </summary>
        public string InputText { get; set; }
    }
    
    /// <summary>
    /// 输入对话框接口
    /// 注：这是一个抽象接口，在具体平台上需要实现相应的UI交互
    /// </summary>
    public abstract class InputDialog
    {
        /// <summary>
        /// 显示输入对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="prompt">提示文本</param>
        /// <returns>对话框结果</returns>
        public abstract InputDialogResult ShowDialog(string title = "", string prompt = "请输入：");
        
        /// <summary>
        /// 控制台实现的简单输入对话框
        /// 注意：这只是一个基本实现，在实际应用中应该在UI平台上实现相应的对话框
        /// </summary>
        public class ConsoleImpl : InputDialog
        {
            public override InputDialogResult ShowDialog(string title = "", string prompt = "请输入：")
            {
                Console.WriteLine($"{title}");
                Console.Write($"{prompt}");
                
                string input = Console.ReadLine();
                
                return new InputDialogResult
                {
                    IsOk = !string.IsNullOrEmpty(input), // 在控制台实现中，非空输入视为确定
                    InputText = input
                };
            }
        }
        
        /// <summary>
        /// 创建默认的输入对话框实例
        /// </summary>
        /// <returns>输入对话框实例</returns>
        public static InputDialog Create()
        {
            // 默认返回控制台实现
            return new ConsoleImpl();
        }
    }
}
