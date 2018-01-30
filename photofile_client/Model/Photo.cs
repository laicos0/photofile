﻿using Newtonsoft.Json;
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
        [JsonProperty(PropertyName = "color")]
        public string PrimaryColor { get; set; }
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
        [JsonProperty(PropertyName = "imageDominantColor")]
        public string DominantColor { get; set; } = "#000000";
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
            Directory.CreateDirectory(Config.PreviewTempPath);
            using (var src = new Mat(OriginalPath)) {
                var aspect = src.Rows / (double)src.Cols;
                var width = Config.PreviewWidth;
                var height = Config.IsKeepAspectRatio ?
                    (width * aspect) : Config.PreviewHeight;
                using (Mat dst = new Mat((int)height, width, src.Type())) {
                    Cv2.Resize(src, dst, dst.Size());
                    dst.SaveImage(PreviewPath);
                }
            }
        }
        /// <summary>
        /// ファイルパス情報を更新します
        /// </summary>
        public void UpdatePath() {
            var basePath = $"{Config.ExportDir}/{Config.GenerateImagedir}";
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
            //一番多い色を取得
            throw new NotImplementedException();
        }
    }
}
