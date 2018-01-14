using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photofile_client.Model {
    [Serializable]
    public class Configuration {
        #region 設定全般
        /// <summary>
        /// 写真の保存先
        /// </summary>
        public string PhotoDir { get; set; } = ".";
        /// <summary>
        /// 出力先
        /// </summary>
        public string ExportDir { get; set; } = "dist";
        /// <summary>
        /// dist/imagesに写真を配置する
        /// </summary>
        public string GenerateImagedir { get; set; } = "images";
        /// <summary>
        /// サムネイル画像の配置先
        /// </summary>
        public string SmallImageDir { get; set; } = "s";
        /// <summary>
        /// Web閲覧画像の配置先
        /// </summary>
        public string MediumImageDir { get; set; } = "m";
        /// <summary>
        /// オリジナルファイルの配置先
        /// </summary>
        public string RawImageDir { get; set; } = "r";

        public int SmallThumbWidth { get; set; } = 300;
        public int SmallThumbHeight { get; set; } = 300;
        public int MediumThumbWidth { get; set; } = 1366;
        public int MediumThumbHeight { get; set; } = 768;
        public bool IsKeepAspectRatio { get; set; } = true;
        #endregion

        #region 操作関連
        public string PreviewTempPath { get; set; } = "tmp";
        public int PreviewWidth { get; set; } = 300;
        public int PreviewHeight { get; set; } = 200;
        public bool IsPreviewKeepAspectRatio { get; set; } = true;
        #endregion

        #region エクスポートしてた前のデータ
        public Photo[] Photos { get; set; } = new Photo[] { };
        public Tag[] Tags { get; set; } = new Tag[] { };
        #endregion
    }
}
