using Newtonsoft.Json;
using photofile_client.Model;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace photofile_client.ViewModel {
    public class MainViewModel {
        #region DataSource
        public ReactiveProperty<string> ConfigPath { get; set; } = new ReactiveProperty<string>("Ready.");
        public ReactiveProperty<string> LogText { get; private set; } = new ReactiveProperty<string>("Ready.");
        public ReactiveProperty<Configuration> Config { get; private set; } = new ReactiveProperty<Configuration>();

        public ReactiveCollection<Photo> Photos { get; private set; } = new ReactiveCollection<Photo>();
        public ReactiveCollection<Photo> FilteredPhotos { get; set; } = new ReactiveCollection<Photo>();
        public ReactiveProperty<Photo> SelectedPhoto { get; set; } = new ReactiveProperty<Photo>();

        public ReactiveCollection<Tag> Tags { get; private set; } = new ReactiveCollection<Tag>();
        #endregion

        #region UI
        public ReactiveProperty<bool> TagFilterAll { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> TagFilterNone { get; set; } = new ReactiveProperty<bool>(false);
        public ReactiveProperty<bool> TagFilterTag { get; set; } = new ReactiveProperty<bool>(false);
        public ReactiveProperty<string> TagFilterSelectedTag { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> ChangeFileName { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> NewTagName { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> NewTagDescription { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<Tag> SelectedTag { get; set; } = new ReactiveProperty<Tag>();

        public ReactiveProperty<bool> IsPhotoUIEnable { get; set; } = new ReactiveProperty<bool>(false);

        #endregion

        #region Commands
        public ReactiveCommand SelectPhotoDirCommand { get; private set; }
        public ReactiveCommand SelectExportDirCommand { get; private set; }
        public ReactiveCommand ReadPhotoCommand { get; private set; }
        public ReactiveCommand ExportCommand { get; private set; }


        public ReactiveCommand ChangeFileNameCommand { get; private set; }
        public ReactiveCommand AddTagCommand { get; private set; }
        public ReactiveCommand<Tag> ToggleTagCommand { get; set; }
        public ReactiveCommand ClosingCommand { get; private set; }
        #endregion

        public MainViewModel() {
            this.ConfigPath.Value = "config.json";
            LoadConfiguration();
            PhotoInitialize();
        }
        /// <summary>
        /// 写真読み込み家の初期化
        /// </summary>
        private void PhotoInitialize() {
            #region ファイル読み込み
            SelectPhotoDirCommand = new ReactiveCommand();
            SelectPhotoDirCommand.Subscribe(() => SelectDirectory(path => {
                Config.Value.PhotoDir = path;
                Config.ForceNotify();
            }));
            SelectExportDirCommand = new ReactiveCommand();
            SelectExportDirCommand.Subscribe(() => SelectDirectory(path => {
                Config.Value.ExportDir = path;
                Config.ForceNotify();
            }));
            ReadPhotoCommand = new ReactiveCommand();
            ReadPhotoCommand.Subscribe(async () => {
                IsPhotoUIEnable.Value = false;

                var photos = await LoadPhotos(new Progress<string>(msg => Log(msg)));
                //反映
                Photos.Clear();
                Tags.Clear();
                Photos.AddRangeOnScheduler(photos);
                Tags.AddRangeOnScheduler(Config.Value.Tags);

                IsPhotoUIEnable.Value = true;
                Log($"{photos.Length}枚の画像を読み込み。プレビュー画像が古い場合、{Config.Value.PreviewTempPath}を削除してから再度実行してください。");
            });
            #endregion

            // フィルタ
            IsPhotoUIEnable.PropertyChangedAsObservable()
                           .Select(x => Photos.AsEnumerable())
                           .CombineLatest(TagFilterAll, TagFilterNone, TagFilterTag, TagFilterSelectedTag,
                                (p, isAll, isNone, isTag, tagText) => {
                                    // RadioButtonが2回判定が来る, trueがふたつあるときは無視
                                    if ((isAll ? 1 : 0)+ (isNone ? 1 : 0) + (isTag ? 1 : 0) > 1) return null;
                                    if (isAll) return p;
                                    else if (isNone) return p.Where(x => x.TagDetails.Length == 0);
                                    else if (isTag) return p.Where(x => x.TagDetails.Any(t => t.ToString().Equals(tagText)));
                                    else throw new NotImplementedException();
                            })
                            .Where(photos => photos != null)
                            .Subscribe(photos => {
                                FilteredPhotos.Clear();
                                FilteredPhotos.AddRangeOnScheduler(photos);
                            });
            #region ファイル名変更
            SelectedPhoto.Where(x => x != null)
                         .Subscribe(p => {
                             ChangeFileName.Value = p.OriginalName;
                         });
            ChangeFileNameCommand =
                ChangeFileName.Select(x => !string.IsNullOrWhiteSpace(x))
                              .ToReactiveCommand();
            ChangeFileNameCommand.Subscribe(() => {
                var oldName = SelectedPhoto.Value.OriginalName;
                var newName = ChangeFileName.Value;
                var newPath = Path.GetFullPath($"{Config.Value.PhotoDir}/{newName}");
                File.Move(SelectedPhoto.Value.OriginalPath, newPath);
                SelectedPhoto.Value.OriginalName = newName;
                Log($"ファイル名を変更 {oldName} -> {newName}");
            });
            #endregion

            #region Tag
            AddTagCommand =
                NewTagName.Select(x => !string.IsNullOrWhiteSpace(x))
                          .ToReactiveCommand(false);

            AddTagCommand.Subscribe(() => {
                var tag = Tags.FirstOrDefault(x => x.Name.Equals(NewTagName.Value));
                if (tag == null) {
                    Tags.AddOnScheduler(new Tag() {
                        CreateAt = DateTime.Now,
                        Name = NewTagName.Value,
                        Description = NewTagDescription.Value,
                    });
                } else {
                    tag.Description = NewTagDescription.Value;
                }
            });
            ToggleTagCommand =
                SelectedPhoto.Select(x => x != null)
                             .ToReactiveCommand<Tag>(false);
            //タグがある場合は削除、なければ追加
            ToggleTagCommand.Subscribe(t => {
                var tagList = SelectedPhoto.Value.TagDetails.ToList();
                var target = tagList.FirstOrDefault(x => x.Name.Equals(t.Name));
                if (target != null) {
                    tagList.Remove(target);
                } else {
                    tagList.Add(t);
                }
                SelectedPhoto.Value.TagDetails = tagList.ToArray();
                SelectedPhoto.ForceNotify();
            });
            #endregion

            #region Export
            ExportCommand = IsPhotoUIEnable.ToReactiveCommand();
            ExportCommand.Subscribe(() => {
                IsPhotoUIEnable.Value = false;
                // 情報のエクスポート
                Config.Value.Photos = Photos.ToArray();
                Config.Value.Tags = Tags.ToArray();
                SaveConfig();

                IsPhotoUIEnable.Value = true;
            });

            #endregion

            ClosingCommand = new ReactiveCommand();
            ClosingCommand.Subscribe(() => {
                SaveConfig();
            });
        }

        private Task<Photo[]> LoadPhotos(IProgress<string> progress) => Task.Run<Photo[]>(() => {
            //一覧の取得
            var regex = new Regex(@"\.(jpg|jpeg)$", RegexOptions.IgnoreCase);
            var photos = Directory.GetFiles(Config.Value.PhotoDir)
                                 .Where(x => regex.IsMatch(x))
                                 .Select(x => new Photo(Config.Value, x))
                                 .Select(x =>
                                     //前回の画像データにあれば置換
                                     Config.Value
                                           .Photos
                                           .FirstOrDefault(p =>
                                                p.OriginalName.Equals(x.OriginalName)
                                           ) ?? x
                                 )
                                 .ToArray();
            if (photos.Length == 0) {
                progress?.Report("ファイルが見つかりませんでした。");
                return new Photo[] { };
            }
            //タグ
            var tags = Config.Value.Tags;
            //プレビュー
            int i = 0;
            foreach (var p in photos) {
                progress?.Report($"プレビュー画像の生成中 ({++i}/{photos.Length}) {p.OriginalName}");
                if (!File.Exists(p.PreviewPath)) {
                    p.GeneratePreviewImage();
                }
            }
            return photos;
        });

        private void SelectDirectory(Action<string> f) {
            //TODO:本当はVMに持たせたくない
            var dialog = new System.Windows.Forms.FolderBrowserDialog() {
                Description = "写真が保存されているディレクトリを選択",
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                f(dialog.SelectedPath);
            }
        }

        /// <summary>
        /// ステータスバーに表示
        /// </summary>
        /// <param name="msg"></param>
        private void Log(string msg) {
            LogText.Value = msg;
        }
        /// <summary>
        /// 設定データを読み込み
        /// </summary>
        private void LoadConfiguration() {
            if (File.Exists(this.ConfigPath.Value)) {
                try {
                    var jsonText = File.ReadAllText(this.ConfigPath.Value);
                    this.Config.Value = JsonConvert.DeserializeObject<Configuration>(jsonText);
                    //Configに保存されている画像にConfig参照をつけておく
                    foreach (var p in this.Config.Value.Photos) {
                        p.Config = this.Config.Value;
                    }
                    Log($"設定データを{ConfigPath.Value}から読み込み完了");
                } catch (Exception ex) {
                    Log(ex.Message);
                }
            } else {
                Config.Value = new Configuration();
                Log("コンフィグを初期値に設定");
            }
        }
        /// <summary>
        /// 設定を保存
        /// </summary>
        private void SaveConfig() {
            try {
                Config.Value.UpdateAt = DateTime.Now;
                var jsonText = JsonConvert.SerializeObject(this.Config.Value, Formatting.Indented);
                File.WriteAllText(ConfigPath.Value, jsonText);
                Log($"設定データを{this.ConfigPath.Value}に保存");
            } catch (Exception ex) {
                Log(ex.Message);
            }
        }

    }
}
