using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace photofile_client.Model {
    [Serializable]
    public class Photo {
        #region 実行中のみ
        /// <summary>
        /// 設定一式
        /// </summary>
        [JsonIgnore]
        public Configuration Config { get; set; }
        /// <summary>
        /// 元画像へのパス
        /// </summary>
        [JsonIgnore]
        public string OriginalPath => Path.GetFullPath($"{Config.PhotoDir}/{OriginalName}");
        /// <summary>
        /// UIプレビュー用の一次画像
        /// </summary>
        [JsonIgnore]
        public string PreviewPath => Path.GetFullPath($"{Config.PreviewTempPath}/{OriginalName}");
        /// <summary>
        /// タグのプレビュー用
        /// </summary>
        [JsonIgnore]
        public string TagText {
            get {
                var sb = new StringBuilder();
                foreach (var t in TagDetails) {
                    sb.Append($"#{t.Name},");
                }
                return sb.ToString();
            }
        }
        #endregion

        #region 保存時に生成
        [JsonProperty(PropertyName = "downloadURL")]
        public string RawPath { get; set; }
        [JsonProperty(PropertyName = "src")]
        public string MediumThumbPath { get; set; }
        [JsonProperty(PropertyName = "srct")]
        public string SmallThumbPath { get; set; }
        [JsonProperty(PropertyName = "imageDominantColor")]
        public string DominantColor { get; set; }
        #endregion

        #region ユーザーが設定
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; } = "";
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = "";
        [JsonProperty(PropertyName = "create_at")]
        public DateTime CreateAt { get; set; } = DateTime.Now;
        [JsonProperty(PropertyName = "tags")]
        public string[] Tags => TagDetails.Select(x => x.Name).ToArray();
        [JsonProperty(PropertyName = "tagDetails")]
        public Tag[] TagDetails { get; set; } = new Tag[] { };

        [JsonProperty(PropertyName = "imgtWidth")]
        public int SmallThumbWidth { get; set; }
        [JsonProperty(PropertyName = "imgtHeight")]
        public int SmallThumbHeight { get; set; }
        [JsonProperty(PropertyName = "width")]
        public int MediumThumbWidth { get; set; }
        [JsonProperty(PropertyName = "height")]
        public int MediumThumbHeight { get; set; }

        public bool IsKeepAspectRatio { get; set; }

        public string OriginalName { get; set; }
        #endregion


        public Photo() { }

        /// <summary>
        /// 新規読み込み
        /// </summary>
        /// <param name="config"></param>
        /// <param name="originPath"></param>
        public Photo(Configuration config, string originPath) {
            Config = config;

            OriginalName = Path.GetFileName(originPath);

            Title = OriginalName;
            SmallThumbWidth = config.SmallThumbWidth;
            SmallThumbHeight = config.SmallThumbHeight;
            MediumThumbWidth = config.MediumThumbWidth;
            MediumThumbHeight = config.MediumThumbHeight;
            IsKeepAspectRatio = config.IsKeepAspectRatio;
        }
        /// <summary>
        /// UIプレビュー用の画像を生成します
        /// </summary>
        public void GeneratePreviewImage() {
            var srcPath = OriginalPath;
            var width = Config.PreviewWidth;
            var originHeight = Config.PreviewHeight;
            var dstPath = PreviewPath;
            var isKeepAspect = Config.IsKeepAspectRatio;

            Directory.CreateDirectory(Config.PreviewTempPath);
            Resize(srcPath, dstPath, width, originHeight, isKeepAspect);
        }

        /// <summary>
        /// srcPathに指定された画像をリサイズする
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="width"></param>
        /// <param name="originHeight"></param>
        /// <param name="dstPath"></param>
        /// <param name="isKeepAspect"></param>
        private static void Resize(string srcPath, string dstPath, int width, int originHeight, bool isKeepAspect) {
            using (var src = new Mat(srcPath)) {
                var aspect = src.Rows / (double)src.Cols;
                var height = isKeepAspect ?
                    (width * aspect) : originHeight;
                using (Mat dst = new Mat((int)height, width, src.Type())) {
                    Cv2.Resize(src, dst, dst.Size());
                    dst.SaveImage(dstPath, new ImageEncodingParam(ImwriteFlags.JpegQuality, 100));
                }
            }
        }

        /// <summary>
        /// ファイルパス情報を更新します
        /// </summary>
        public void UpdatePath() {
            var basePath = $"{Config.ExportDir}/{Config.GenerateImageDir}";
            RawPath = $"{basePath}/{Config.RawImageDir}/{OriginalName}";
            MediumThumbPath = $"{basePath}/{Config.MediumImageDir}/{OriginalName}";
            SmallThumbPath = $"{basePath}/{Config.SmallImageDir}/{OriginalName}";
        }
        /// <summary>
        /// 画像を生成します
        /// </summary>
        public void GenerateImage() {
            //事前にパスを新しくしておく
            UpdatePath();
            //リサイズした画像を生成する

            //origin
            Directory.CreateDirectory($"{Config.ExportDir}/{Config.GenerateImageDir}/{Config.RawImageDir}");
            File.Copy(OriginalPath, RawPath, true);
            // medium
            Directory.CreateDirectory($"{Config.ExportDir}/{Config.GenerateImageDir}/{Config.MediumImageDir}");
            Resize(OriginalPath, MediumThumbPath, this.MediumThumbWidth, this.MediumThumbHeight, this.IsKeepAspectRatio);
            // small
            Directory.CreateDirectory($"{Config.ExportDir}/{Config.GenerateImageDir}/{Config.SmallImageDir}");
            Resize(OriginalPath, SmallThumbPath, this.SmallThumbWidth, this.SmallThumbHeight, this.IsKeepAspectRatio);

            //一番多い色を取得
            using (var src = new Mat(OriginalPath))
            using (var resized = src.Resize(new Size(1, 1))) {
                var mean = resized.Mean();
                DominantColor = $"#{(int)mean[2]:X02}{(int)mean[1]:X02}{(int)mean[0]:X02}";//BGT -> RGB

            }
        }
    }
}
