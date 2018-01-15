﻿using Newtonsoft.Json;
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

namespace photofile_client.ViewModel {
    public class MainViewModel {
        #region DataSource
        public ReactiveProperty<string> ConfigPath { get; set; } = new ReactiveProperty<string>("Ready.");
        public ReactiveProperty<string> LogText { get; private set; } = new ReactiveProperty<string>("Ready.");
        public ReactiveProperty<Configuration> Config { get; private set; } = new ReactiveProperty<Configuration>();
        public ReactiveCollection<Photo> Photos { get; private set; } = new ReactiveCollection<Photo>();
        public ReactiveCollection<Photo> SelectedPhotos { get; set; }

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

        public ReactiveCollection<string> SelectedTags { get; set; } = new ReactiveCollection<string>();

        public ReactiveProperty<bool> IsPhotoUIEnable { get; set; } = new ReactiveProperty<bool>(false);
        #endregion

        #region Commands
        public ReactiveCommand SelectPhotoDirCommand { get; private set; }
        public ReactiveCommand SelectExportDirCommand { get; private set; }
        public ReactiveCommand ReadPhotoCommand { get; private set; }
        public ReactiveCommand ExportCommand { get; private set; }

        public ReactiveCommand PhotoSelectAllCommand { get; private set; }
        public ReactiveCommand PhotoUnSelectAllCommand { get; private set; }

        public ReactiveCommand ChangeFileNameCommand { get; private set; }
        public ReactiveCommand AddTagCommand { get; private set; }
        #endregion

        public MainViewModel() {
            this.ConfigPath.Value = "config.json";
            LoadConfiguration();

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
        }

        private Task<Photo[]> LoadPhotos(IProgress<string> progress) => Task.Run<Photo[]>(() => {
            //一覧の取得
            var regex = new Regex(@"\.(jpg|JPG|jpeg|JPEG)$");
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
                    Log("設定データを読み込み完了");
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
                var jsonText = JsonConvert.SerializeObject(this.Config.Value);
                Log($"設定データを{this.ConfigPath.Value}に保存");
            } catch (Exception ex) {
                Log(ex.Message);
            }
        }

    }
}
