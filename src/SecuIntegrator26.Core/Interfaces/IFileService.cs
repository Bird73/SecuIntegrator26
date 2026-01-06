using System.IO;
using System.Threading.Tasks;

namespace SecuIntegrator26.Core.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// 儲存文字內容到指定檔案
        /// </summary>
        Task SaveTextAsync(string relativePath, string content);

        /// <summary>
        /// 儲存二進位內容到指定檔案
        /// </summary>
        Task SaveBytesAsync(string relativePath, byte[] content);

        /// <summary>
        /// 讀取檔案文字內容 (若不存在回傳 null)
        /// </summary>
        Task<string?> ReadTextAsync(string relativePath);

        /// <summary>
        /// 檢查檔案是否存在
        /// </summary>
        bool Exists(string relativePath);

        /// <summary>
        /// 取得檔案完整路徑
        /// </summary>
        string GetFullPath(string relativePath);
        
        /// <summary>
        /// 確保目錄存在
        /// </summary>
        void EnsureDirectory(string relativePath);
    }
}
