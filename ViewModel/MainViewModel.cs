using CBakWeChatDesktop.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBakWeChatDesktop.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        // session 列表
        [ObservableProperty]
        private ObservableCollection<Session>? sessions = null;
        // 当前展示 session
        [ObservableProperty]
        private Session? session = null;
        [ObservableProperty]
        private string eventDesc = string.Empty;
        [ObservableProperty]
        private string eventTitle = string.Empty;
        [ObservableProperty]
        private string version = string.Empty;
        [ObservableProperty]
        private string lastSyncTime = string.Empty;
    }
}
