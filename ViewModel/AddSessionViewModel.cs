using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBakWeChatDesktop.Model;

namespace CBakWeChatDesktop.ViewModel
{
    public partial class AddSessionViewModel : ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<ProcessInfo> processes = new ObservableCollection<ProcessInfo>();

        [ObservableProperty]
        private ProcessInfo? selectedProcess;

        [ObservableProperty]
        private Session? session;

        // 用户添加 session 时的 session 名
        [ObservableProperty]
        private string sessionName = string.Empty;
        
        // session 描述
        [ObservableProperty]
        private string sessionDesc = string.Empty;

        [ObservableProperty]
        private string eventDesc = string.Empty;

    }
}
