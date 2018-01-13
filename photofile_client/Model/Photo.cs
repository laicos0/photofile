using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photofile_client.Model {
    [Serializable]
    public class Photo {
        #region 保存時に生成
        [JsonProperty(PropertyName = "srcr")]
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
        [JsonProperty(PropertyName = "create_at")]
        public DateTime CreateAt { get; set; } = DateTime.Now;
        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; set; } = new string[] { };

        public int SmallThumbWidth { get; set; }
        public int SmallThumbHeight { get; set; }
        public int MediumThumbWidth { get; set; }
        public int MediumThumbHeight { get; set; }
        public bool IsHoldAspectRatio { get; set; }

        [JsonIgnore]
        public string OriginalPath { get; set; }
        public string OriginalName { get; set; }

        #endregion

        public Photo() { }
        public Photo(Configuration config, string originPath) {
            OriginalPath = originPath;
            OriginalName = Path.GetFileName(OriginalPath);

            SmallThumbWidth = config.SmallThumbWidth;
            SmallThumbHeight = config.SmallThumbHeight;
            MediumThumbWidth = config.MediumThumbWidth;
            MediumThumbHeight = config.MediumThumbHeight;
            IsHoldAspectRatio = config.IsHoldAspectRatio;
        }
        /// <summary>
        /// ファイルパス情報を更新します
        /// </summary>
        /// <param name="config"></param>
        public void UpdatePath(Configuration config) {
            var basePath = $"{config.ExportDir}/{config.GenerateImagedir}";
            RawPath = $"{basePath}/{config.RawImageDir}/{OriginalName}";
            MediumThumbPath = $"{basePath}/{config.MediumImageDir}/{OriginalName}";
            SmallThumbPath = $"{basePath}/{config.SmallImageDir}/{OriginalName}";
        }
        /// <summary>
        /// 画像を生成します
        /// </summary>
        /// <param name="config"></param>
        public void GenerateImage(Configuration config) {
            //事前にパスを新しくしておく
            UpdatePath(config);
            //リサイズした画像を生成する
            //一番多い色を取得
            throw new NotImplementedException();
        }
    }
}
