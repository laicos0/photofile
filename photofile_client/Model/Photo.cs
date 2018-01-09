using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photofile_client.Model {
    [Serializable]
    class Photo {
        #region 保存時に生成
        [JsonProperty(PropertyName = "srcr")]
        public string RawPath { get; set; }
        [JsonProperty(PropertyName = "src")]
        public string MediumThumbPath { get; set; }
        [JsonProperty(PropertyName = "srct")]
        public string SmallThumbPath { get; set; }
        #endregion

        #region ユーザーが設定
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "create_at")]
        public DateTime CreateAt { get; set; }
        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; set; }

        public int SmallThumbWidth { get; set; }
        public int SmallThumbHeight { get; set; }
        public int MediumThumbWidth { get; set; }
        public int MediumThumbHeight { get; set; }
        public bool IsHoldAspectRatio { get; set; }
        public string OriginalName { get; set; }
        #endregion
    }
}
