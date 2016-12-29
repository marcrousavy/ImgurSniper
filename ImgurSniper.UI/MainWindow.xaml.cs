﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImgurSniper.UI {
    public partial class MainWindow : Window {
        public InstallerHelper helper;


        //Path to Program Files/ImgurSniper Folder
        private string _path {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ImgurSniper");
                return value;
            }
        }

        //Path to Documents/ImgurSniper Folder
        private string _docPath {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                return value;
            }
        }

        //Animation Templates
        private DoubleAnimation _fadeOut {
            get {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = 1;
                anim.To = 0;
                anim.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                return anim;
            }
        }
        private DoubleAnimation _fadeIn {
            get {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = 0;
                anim.To = 1;
                anim.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                return anim;
            }
        }
        private ImgurLoginHelper _imgurhelper;


        public MainWindow() {
            InitializeComponent();
            this.Closing += WindowClosing;

            if(!Directory.Exists(_path)) {
                Directory.CreateDirectory(_path);
                NewToImgur();
            }

            if(!Directory.Exists(_docPath))
                Directory.CreateDirectory(_docPath);

            helper = new InstallerHelper(_path, error_toast, success_toast, this);
            _imgurhelper = new ImgurLoginHelper(error_toast, success_toast);

            error_toast.Show("Loading...", TimeSpan.FromSeconds(2));
            Load();
        }


        private async void NewToImgur() {
            await Task.Delay(500);
            success_toast.Show("Hi! You're new to ImgurSniper! Start by clicking \"Install\" first!", TimeSpan.FromSeconds(2));
        }



        private async void Load() {
            string[] lines = FileIO.ReadConfig();

            for(int i = 0; i < lines.Length; i++) {
                try {
                    string property = lines[i].Split(':')[0];
                    string value = lines[i].Split(':')[1];

                    switch(property) {
                        case "AfterSnipeAction":
                            if(value == "Clipboard") {
                                ClipboardRadio.IsChecked = true;
                            } else {
                                ImgurRadio.IsChecked = true;
                            }
                            break;
                        case "SaveImages":
                            SaveBox.IsChecked = bool.Parse(value);
                            break;
                        case "Magnifyer":
                            MagnifyingGlassBox.IsChecked = bool.Parse(value);
                            break;
                        case "SnipeMonitor":
                            if(value == "All") {
                                MultiMonitorRadio.IsChecked = true;
                            } else {
                                CurrentMonitorRadio.IsChecked = true;
                            }
                            break;
                    }
                } catch(Exception) { }
            }

            string refreshToken = FileIO.ReadRefreshToken();
            string name = await _imgurhelper.LoggedInUser(refreshToken);

            if(name != null) {
                Label_Account.Content = Label_Account.Content as string + " (Logged In as " + name + ")";

                Btn_SignIn.Visibility = Visibility.Collapsed;
                Btn_SignOut.Visibility = Visibility.Visible;
            }
        }


        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Closing -= WindowClosing;

            DoubleAnimation fadingAnimation = new DoubleAnimation();
            fadingAnimation.From = 1;
            fadingAnimation.To = 0;
            fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            fadingAnimation.AutoReverse = false;
            fadingAnimation.Completed += delegate {
                this.Close();
            };

            grid.BeginAnimation(Grid.OpacityProperty, fadingAnimation);
        }


        private void AfterSnapClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.AfterSnipeAction, button.Tag as string);
                } catch(Exception) { }
            }
        }

        private void MonitorsClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.SnipeMonitor, button.Tag as string);
                } catch(Exception) { }
            }
        }


        private void SaveImgs_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.SaveImages, box.IsChecked.ToString());
                } catch(Exception) { }
            }
        }


        private void Magnifying_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.Magnifyer, box.IsChecked.ToString());
                } catch(Exception) { }
            }
        }

        private void Snipe(object sender, RoutedEventArgs e) {
            string exe = Path.Combine(_path, "ImgurSniper.exe");

            if(File.Exists(exe)) {
                Process.Start(exe);
            } else {
                error_toast.Show("Error, ImgurSniper could not be found on your System!",
                    TimeSpan.FromSeconds(3));
            }
        }

        private void Install(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                helper.Install(sender);
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void Uninstall(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            helper.Uninstall();
        }
        private void DesktopShortcut(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                helper.AddToDesktop(sender);
                success_toast.Show("Created Desktop Shortcut!", TimeSpan.FromSeconds(1));
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void StartmenuShortcut(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                helper.AddToStartmenu(sender);
                success_toast.Show("Created Startmenu Shortcut!", TimeSpan.FromSeconds(1));
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Enable or disable Buttons
        /// </summary>
        public void ChangeButtonState(bool enabled) {
            if(Btn_Desktop.Tag == null)
                Btn_Desktop.IsEnabled = enabled;

            if(Btn_Install.Tag == null)
                Btn_Install.IsEnabled = enabled;

            if(Btn_Snipe.Tag == null)
                Btn_Snipe.IsEnabled = enabled;

            if(Btn_Startmenu.Tag == null)
                Btn_Startmenu.IsEnabled = enabled;

            if(Btn_Uninstall.Tag == null)
                Btn_Uninstall.IsEnabled = enabled;
        }

        private void SignIn(object sender, RoutedEventArgs e) {
            try {
                _imgurhelper.Authorize();

                DoubleAnimation fadeBtnOut = _fadeOut;
                fadeBtnOut.Completed += delegate {

                    DoubleAnimation fadePanelIn = _fadeIn;
                    fadePanelIn.Completed += delegate {
                        Btn_SignIn.Visibility = Visibility.Collapsed;
                    };
                    Panel_PIN.Visibility = Visibility.Visible;
                    Panel_PIN.BeginAnimation(StackPanel.OpacityProperty, fadePanelIn);

                };
                Btn_SignIn.BeginAnimation(Button.OpacityProperty, fadeBtnOut);
            } catch(Exception) { }
        }
        private void SignOut(object sender, RoutedEventArgs e) {
            DoubleAnimation fadeBtnOut = _fadeOut;
            fadeBtnOut.Completed += delegate {
                FileIO.DeleteToken();

                DoubleAnimation fadeBtnIn = _fadeIn;
                fadeBtnIn.Completed += delegate {
                    Btn_SignOut.Visibility = Visibility.Collapsed;

                    Label_Account.Content = "Imgur Account";
                };
                Btn_SignIn.Visibility = Visibility.Visible;
                Btn_SignIn.BeginAnimation(StackPanel.OpacityProperty, fadeBtnIn);

            };
            Btn_SignOut.BeginAnimation(Button.OpacityProperty, fadeBtnOut);
        }

        private async void PINOk(object sender, RoutedEventArgs e) {
            bool result = await _imgurhelper.Login(Box_PIN.Text);

            if(result) {
                DoubleAnimation fadePanelOut = _fadeOut;
                fadePanelOut.Completed += delegate {
                    DoubleAnimation fadeBtnIn = _fadeIn;
                    fadeBtnIn.Completed += delegate {
                        Panel_PIN.Visibility = Visibility.Collapsed;
                    };
                    Btn_SignOut.Visibility = Visibility.Visible;
                    Btn_SignOut.BeginAnimation(StackPanel.OpacityProperty, fadeBtnIn);

                };
                Panel_PIN.BeginAnimation(Button.OpacityProperty, fadePanelOut);


                string refreshToken = FileIO.ReadRefreshToken();
                string name = await _imgurhelper.LoggedInUser(refreshToken);

                if(name != null) {
                    Label_Account.Content = Label_Account.Content as string + " (Logged In as " + name + ")";

                    Btn_SignIn.Visibility = Visibility.Collapsed;
                    Btn_SignOut.Visibility = Visibility.Visible;
                }
                Box_PIN.Clear();
            }
        }

        private void Box_PIN_TextChanged(object sender, TextChangedEventArgs e) {
            if(Box_PIN.Text.Length > 0) {
                Btn_PinOk.IsEnabled = true;
            } else {
                Btn_PinOk.IsEnabled = false;
            }
        }
    }
}